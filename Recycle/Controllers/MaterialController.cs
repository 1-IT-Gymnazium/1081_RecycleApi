using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Materials;
using Recycle.Api.Models.Products;
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
    public MaterialController(ILogger<MaterialController> logger, IClock clock, AppDbContext dbContext)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
    }
    [HttpPost("api/v1/Material/")]
    public async Task<ActionResult> Create([FromBody] MaterialCreateModel model)
    {
        var newEntity = new Material
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description,
        };
        _dbContext.Add(newEntity);
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

        //[HttpPatch("api/v1/Material/{id:Guid}")]

        //public async Task<ActionResult<MaterialDetailModel>> UpdateMaterial(
        //    [FromRoute] Guid id,
        //    [FromBody] JsonPatchDocument<MaterialDetailModel> patch)
        //{
        //    return Ok(patch);
        //
    }
    [HttpDelete("api/v1/Product/{Id:Guid}")]
        public async Task<ActionResult> DeleteMaterial(
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

