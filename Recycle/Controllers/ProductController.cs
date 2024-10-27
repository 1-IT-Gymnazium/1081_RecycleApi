using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Recycle.Api.Models.Products;
using Recycle.Data;
using Recycle.Data.Entities;

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
}
