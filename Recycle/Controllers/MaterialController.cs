using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Materials;
using Recycle.Api.Models.Products;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;
using System.Security.Cryptography.X509Certificates;

namespace Recycle.Api.Controllers;

[ApiController]

public class MaterialController : ControllerBase
{
    private readonly ILogger<MaterialController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;
    private readonly IApplicationMapper _mapper;
    public MaterialController(ILogger<MaterialController> logger, IClock clock, AppDbContext dbContext, IApplicationMapper mapper)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
        _mapper = mapper;
    }
    [HttpPost("api/v1/Material/")]
    public async Task<ActionResult> Create([FromBody] MaterialCreateModel model)
    {
        var now = _clock.GetCurrentInstant();
        var newMaterial = new Material
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description,
        };
        _dbContext.Add(newMaterial);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    [HttpGet("api/v1/Material/")]
    public async Task<ActionResult<List<MaterialDetailModel>>> GetList()
    {
        var dbEntities = _dbContext.Materials.ToList();

        return Ok(dbEntities);
    }
    [HttpPost("api/v1/Material/{Guid:Id}")]
    public async Task<ActionResult<MaterialDetailModel>> GetById(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<Material>()
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        };
        var material = new MaterialCreateModel
        {
            Id = dbEntity.Id,
            Name = dbEntity.Name,
            Description = dbEntity.Description,
        };
        return Ok(material);
    }

        [HttpPatch("api/v1/Product{id:Guid}")]

        public async Task<ActionResult<MaterialDetailModel>> UpdateMaterial(
            [FromRoute] Guid id,
            [FromBody] JsonPatchDocument<MaterialDetailModel> patch)
        {
            var dbEntity = await _dbContext
                .Set<Material>()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dbEntity == null)
            {
                return NotFound();
            }
            var materialToUpdate = dbEntity.ToDetail();
            patch.ApplyTo(materialToUpdate);
            var uniqueCheck = await _dbContext
                .Set<Product>()
                .AnyAsync(x => x.Id == materialToUpdate.Id);
            if (uniqueCheck)
            {
                ModelState.AddModelError<ProductDetailModel>(x => x.EAN, "Ean is not unique");
            }
            if (!(ModelState.IsValid && TryValidateModel(materialToUpdate)))
            {
                return ValidationProblem(ModelState);
            }
            dbEntity.Id = materialToUpdate.Id;
            dbEntity.Name = materialToUpdate.Name;
            dbEntity.Description = materialToUpdate.Description;

            await _dbContext.SaveChangesAsync();

            dbEntity = await _dbContext
                .Set<Material>()
                .FirstOrDefaultAsync(x => x.Id == id);

            return Ok(dbEntity.ToDetail());
        }
        [HttpDelete("api/v1/Product/{Id:Guid}")]
        public async Task<IActionResult> DeleteMaterial(
        [FromRoute] Guid id)
        {
        var dbEntity = await _dbContext
            .Set<Material>()
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

