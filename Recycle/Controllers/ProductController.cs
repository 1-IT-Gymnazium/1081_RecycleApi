using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Products;
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

    public ProductController(ILogger<ProductController> logger, IClock clock, AppDbContext dbContext)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
    }
    [HttpPost("api/v1/Product/")]
    public async Task<ActionResult> Create([FromBody] ProductCreateModel model)
    {
        var newEntity = new Product
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            EAN = model.EAN,
            Description = model.Description,
            PicturePath = model.PicturePath,
            CreatedBy = model.CreatedBy,
            ModifiedBy = model.ModifiedBy
        };
        _dbContext.Add(newEntity);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("api/v1/Product/")]
    public async Task<ActionResult<List<ProductDetailModel>>> GetList()
    {
        var dbEntities = _dbContext.Products.ToList();

        return Ok(dbEntities);
    }
    [HttpGet("api/v1/Product{Id:Guid}")]
    public async Task<ActionResult<ProductCreateModel>> GetById(
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
        var product = new ProductCreateModel
        {
           Id = dbEntity.Id,
           EAN = dbEntity.EAN,
           Name = dbEntity.Name,
           Description = dbEntity.Description,
           PicturePath = dbEntity.PicturePath,
           CreatedBy = dbEntity.CreatedBy,
           ModifiedBy = dbEntity.ModifiedBy,
        };
        return Ok(product);
    }
    [HttpPatch("api/v1/Product{id:Guid}")]
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
        var productToUpdate = dbEntity.ToDetail();
        patch.ApplyTo(productToUpdate);
        var uniqueCheck = await _dbContext
            .Set<Product>()
            .AnyAsync(x => x.EAN == productToUpdate.EAN);
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

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Product>()
            .FirstOrDefaultAsync(x => x.Id == id);

        return Ok(dbEntity.ToDetail());
    }
    [HttpDelete("api/v1/Product/{Id:Guid}")]
    public async Task<ActionResult> DeleteProduct(
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
