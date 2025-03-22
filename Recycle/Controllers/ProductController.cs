using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Parts;
using Recycle.Api.Models.Products;
using Recycle.Api.Services;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Recycle.Api.Controllers;

/// <summary>
/// Controller for managing product entities, including creation, editing, searching by EAN,
/// part composition, image uploads and soft deletes.
/// </summary>
[ApiController]

public class ProductController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;
    private readonly IApplicationMapper _mapper;
    private readonly IImageService _imageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductController"/> class with required services.
    /// </summary>
    public ProductController(ILogger<ProductController> logger, IClock clock, AppDbContext dbContext, IApplicationMapper mapper, IImageService imageService)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
        _mapper = mapper;
        _imageService = imageService;
    }
    /// <summary>
    /// Creates a new product with its assigned parts.
    /// </summary>
    /// <param name="model">The product creation model containing name, description, EAN, image and part IDs.</param>
    /// <returns>
    /// Returns 201 (Created) with product detail, or 400 (Bad Request) if name or parts are invalid.
    /// </returns>

    [Authorize]
    [HttpPost("api/v1/Product/")]
    public async Task<ActionResult> CreateProduct([FromBody] ProductCreateModel model)
    {
        var checkProduct = await _dbContext
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
            IsVerified = false,
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
            newProduct.ProductParts.Add(new() { PartId = id, ProductId = newProduct.Id });
        }
        await _dbContext.AddAsync(newProduct);
        await _dbContext.SaveChangesAsync();

        newProduct = await _dbContext
            .Products
            .Include(x => x.ProductParts)
                .ThenInclude(x => x.Part)
                    .ThenInclude(x => x.PartMaterial)
                        .ThenInclude(x => x.TrashCanMaterials)
                            .ThenInclude(x => x.TrashCan)
            .FirstAsync(x => x.Id == newProduct.Id);

        var url = Url.Action(nameof(GetProductById), new { newProduct.Id })
            ?? throw new Exception("failed to generate url");
        return Created(url, _mapper.ToDetail(newProduct));
    }

    /// <summary>
    /// Uploads an image for a product and saves it to the server.
    /// </summary>
    /// <param name="productImage">The uploaded product image file.</param>
    /// <returns>
    /// Returns 200 (OK) with the saved image path, or 400 (Bad Request) if upload fails.
    /// </returns>
    [HttpPost("api/v1/Product/UploadProductImage")]
    public async Task<IActionResult> UploadProductImage(IFormFile productImage)
    {
        if (productImage == null || productImage.Length == 0)
        {
            return BadRequest(new { error = "NO_FILE_UPLOADED", message = "No product image uploaded." });
        }

        // Save the image using the ImageService
        var newImagePath = await _imageService.SaveImageAsync(productImage, "ProductImages");

        // Return the stored image path
        return Ok(new { message = "Product image uploaded successfully.", imagePath = newImagePath });
    }

    /// <summary>
    /// Retrieves a list of all products, including their parts and related materials and trash cans.
    /// </summary>
    /// <returns>
    /// Returns 200 (OK) with the list of products.
    /// </returns>
    [HttpGet("api/v1/Product/")]
    public async Task<ActionResult<List<ProductDetailModel>>> GetListProduct()
    {
        var dbEntities = _dbContext
            .Products
            .Include(x => x.ProductParts)
                .ThenInclude(x => x.Part)
                    .ThenInclude(x => x.PartMaterial)
                        .ThenInclude(x => x.TrashCanMaterials)
                            .ThenInclude(x => x.TrashCan)
            .FilterDeleted()
            .Select(_mapper.ToDetail);

        return Ok(dbEntities);
    }

    /// <summary>
    /// Retrieves a specific product by its ID, including parts, materials, and trash can info.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>
    /// Returns 200 (OK) with product detail, or 404 (Not Found) if the product does not exist.
    /// </returns>
    [HttpGet("api/v1/Product/{id:guid}")]
    public async Task<ActionResult<ProductDetailModel>> GetProductById(
        [FromRoute] Guid id
        )
    {
        var dbEntity = await _dbContext
            .Set<Product>()
            .Include(x => x.ProductParts)
                .ThenInclude(x => x.Part)
                    .ThenInclude(x => x.PartMaterial)
                        .ThenInclude(x => x.TrashCanMaterials)
                            .ThenInclude(x => x.TrashCan)
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        return Ok (_mapper.ToDetail(dbEntity));
    }

    /// <summary>
    /// Searches for products based on the given EAN code.
    /// </summary>
    /// <param name="ean">The EAN code to search by.</param>
    /// <returns>
    /// Returns 200 (OK) with matching products, or 404 (Not Found) if no match is found.
    /// </returns>
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
            .Include (x => x.ProductParts)
            .ThenInclude(x => x.Part)
            .FilterDeleted() 
            .Where(x => x.EAN == ean)
            .ToListAsync();

        if (dbEntities == null || !dbEntities.Any())
        {
            return NotFound();
        }

        return Ok(dbEntities.Select(x => _mapper.ToDetail(x)));
    }

    /// <summary>
    /// Updates a product using a JSON Patch document. Also updates part assignments.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="patch">Patch document containing updated fields.</param>
    /// <returns>
    /// Returns 200 (OK) with updated product detail, 404 (Not Found) if not found, 
    /// or 400 (Bad Request) if validation fails.
    /// </returns>
    [Authorize]
    [HttpPatch("api/v1/Product/{id:guid}")]
    public async Task<ActionResult<ProductUpdateModel>> UpdateProduct(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<ProductUpdateModel> patch)
    {
        var dbEntity = await _dbContext
            .Set<Product>()
            .Include(x => x.ProductParts)
                .ThenInclude(x => x.Part)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (dbEntity == null)
        {
            return NotFound();
        }

        var productToUpdate = _mapper.ToUpdate(dbEntity);
        patch.ApplyTo(productToUpdate);

        var uniqueCheck = await _dbContext
            .Set<Product>()
            .AnyAsync(x => x.Id != id && x.EAN == productToUpdate.EAN);

        if (uniqueCheck)
        {
            ModelState.AddModelError<ProductUpdateModel>(x => x.EAN, "Ean is not unique");
        }

        if (!(ModelState.IsValid && TryValidateModel(productToUpdate)))
        {
            return ValidationProblem(ModelState);
        }

        dbEntity.IsVerified = productToUpdate.IsVerified;
        dbEntity.EAN = productToUpdate.EAN;
        dbEntity.Name = productToUpdate.Name;
        dbEntity.Description = productToUpdate.Description;
        dbEntity.PicturePath = productToUpdate.PicturePath;

        // Handle Product Parts
        var currentParts = dbEntity.ProductParts.ToList();
        var updatedParts = productToUpdate.Parts.Select(p => p.Id) ?? new List<Guid>();

        var removedParts = currentParts.Where(x => !updatedParts.Contains(x.PartId)).ToList();
        var newParts = updatedParts.Where(x => !currentParts.Any(y => y.PartId == x)).ToList();

        foreach (var part in removedParts)
        {
            dbEntity.ProductParts.Remove(part);
        }

        foreach (var partId in newParts)
        {
            var partExists = await _dbContext.Parts.AnyAsync(x => x.Id == partId);
            if (!partExists)
            {
                ModelState.AddModelError(nameof(productToUpdate.Parts), $"Part with id {partId} not found");
                return ValidationProblem(ModelState);
            }
            dbEntity.ProductParts.Add(new ProductPart { PartId = partId, ProductId = dbEntity.Id });
        }

        dbEntity.SetModifyBySystem(_clock.GetCurrentInstant());
        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Product>()
            .Include(x => x.ProductParts)
                .ThenInclude(x => x.Part)
                    .ThenInclude(x => x.PartMaterial)
                        .ThenInclude(x => x.TrashCanMaterials)
                            .ThenInclude(x => x.TrashCan)

            .FirstAsync(x => x.Id == id);

        return Ok(_mapper.ToDetail(dbEntity));
    }

    /// <summary>
    /// Soft deletes a product by marking it as deleted.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <returns>
    /// Returns 204 (No Content) on success, or 404 (Not Found) if product does not exist.
    /// </returns>
    [Authorize]
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
