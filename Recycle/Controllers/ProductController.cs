using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Products;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Recycle.Api.Controllers;

[ApiController]

public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;
    private readonly IApplicationMapper _mapper;

    public ProductController(ILogger<ProductController> logger, IClock clock, AppDbContext dbContext, IApplicationMapper mapper)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
        _mapper = mapper;
    }
    /// <summary>
    /// Creates a new product entity from model
    /// </summary>
    /// <param name="model">
    ///Data model which is providing informations about the created Product
    /// </param>
    /// <returns>
    /// ActionResult is idicating 
    /// </returns>
    [HttpPost("api/v1/Product/")]
    public async Task<ActionResult> CreateProduct([FromBody] ProductCreateModel model)
    {
        var checkProduct =
            await _dbContext
            .Set<Product>()
            .AnyAsync(x => x.Name == model.Name);
        if (checkProduct)
        {
            ModelState
                   .AddModelError(nameof(model.Name), $"Product with the name of {model.Name} already exists!");
            return ValidationProblem(ModelState);
        }
        
        var now = _clock.GetCurrentInstant();
        var newProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            EAN = model.EAN,
            Description = model.Description,
            PicturePath = model.PicturePath,
            ProductParts = new List<ProductPart>()
        }
        .SetCreateBySystem(now);
        foreach (var id in model.PartIds)
        {
            var part = await _dbContext.Parts.FirstOrDefaultAsync(x => x.Id == id);
            if (part == null)
            {
                ModelState
                    .AddModelError(nameof(model.PartIds), $"Part with id {id} not found");
            }
            newProduct.ProductParts.Add(new() { PartId = id, ProductId=newProduct.Id });
        }
        await _dbContext.AddAsync(newProduct);
        await _dbContext.SaveChangesAsync();

        newProduct = await _dbContext
            .Products
            .FirstAsync(x => x.Id == newProduct.Id);

        //create ProductParts in DB
        foreach (var productPart in newProduct.ProductParts)
        {
            var newProductPart = new ProductPart
            {
                Id = Guid.NewGuid(),
                ProductId = productPart.ProductId,
                PartId = productPart.PartId
            };
            await _dbContext.AddAsync(newProductPart);
            await _dbContext.SaveChangesAsync();

        }

        var url = Url.Action(nameof(GetProductById), new { newProduct.Id })
            ?? throw new Exception("failed to generate url");
        return Created(url, _mapper.ToDetail(newProduct));
    }

    [HttpGet("api/v1/Product/")]
    public async Task<ActionResult<List<ProductDetailModel>>> GetListProduct()
    {
        var dbEntities = _dbContext
            .Products
            .FilterDeleted()
            .Select(_mapper.ToDetail);

        return Ok(dbEntities);
    }
    [HttpGet("api/v1/Product/{id:guid}")]
    public async Task<ActionResult<ProductDetailModel>> GetProductById(
        [FromRoute] Guid id
        )
    {
        var dbEntity = await _dbContext
            .Set<Product>()
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        };
        var product = new ProductDetailModel
        {
           Id = dbEntity.Id,
           EAN = dbEntity.EAN,
           Name = dbEntity.Name,
        };
        return Ok(product);
    }
    [HttpGet("api/v1/Product/search")]
    public async Task<ActionResult<IEnumerable<ProductDetailModel>>> SearchProductsByEAN(
        [FromQuery] string ean
    )
    {
        if (string.IsNullOrEmpty(ean))
        {
            return BadRequest("EAN cannot be null or empty.");
        }

        var dbEntities = await _dbContext
            .Set<Product>()
            .FilterDeleted() 
            .Where(x => x.EAN == ean)
            .ToListAsync();

        if (dbEntities == null || !dbEntities.Any())
        {
            return NotFound();
        }

        var products = dbEntities.Select(dbEntity => new ProductDetailModel
        {
            Id = dbEntity.Id,
            EAN = dbEntity.EAN,
            Name = dbEntity.Name,
        }).ToList();

        return Ok(products);
    }

    [HttpPatch("api/v1/Product/{id:guid}")]
    public async Task<ActionResult<ProductDetailModel>> UpdateProduct(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<ProductDetailModel> patch)
    {
        var dbEntity = await _dbContext
            .Set<Product>()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }

        var productToUpdate = _mapper.ToDetail(dbEntity);
        patch.ApplyTo(productToUpdate);
        var uniqueCheck = await _dbContext
            .Set<Product>()
            .AnyAsync(x => x.Id != id && x.EAN == productToUpdate.EAN);
        if (uniqueCheck)
        {
            ModelState.AddModelError<ProductDetailModel>(x => x.EAN, "Ean is not unique");
        }
        if (!(ModelState.IsValid && TryValidateModel(productToUpdate)))
        {
            return ValidationProblem(ModelState);
        }
        dbEntity.EAN = productToUpdate.EAN;
        dbEntity.Name = productToUpdate.Name;
        dbEntity.Description = productToUpdate.Description;
        dbEntity.PicturePath = productToUpdate.PicturePath;
        dbEntity.SetModifyBySystem(_clock.GetCurrentInstant());

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Product>()
            .FirstOrDefaultAsync(x => x.Id == id);

        return Ok(_mapper.ToDetail(dbEntity));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("api/v1/Product/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<Product>()
            .FilterDeleted()
            .SingleOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        dbEntity.SetDeleteBySystem(_clock.GetCurrentInstant());
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
