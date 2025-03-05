using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Parts;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Recycle.Api.Controllers;
[ApiController]
public class PartController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;
    private readonly IApplicationMapper _mapper;

    public PartController(ILogger<ProductController> logger, IClock clock, AppDbContext dbContext, IApplicationMapper mapper)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
        _mapper = mapper;
    }
    [Authorize]
    [HttpPost("api/v1/Part/")]
    public async Task<ActionResult> CreatePart([FromBody] PartCreateModel model)
    {
    var checkPart = await _dbContext
    .Set<Part>()
    .AnyAsync(x => x.Name == model.Name);
        if (checkPart)
        {
            ModelState
                   .AddModelError(nameof(model.Name), $"Part with the name of {model.Name} already exists!");
            return ValidationProblem(ModelState);
        }

        var now =_clock.GetCurrentInstant();
        var newPart = new Part
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description,
            PicturePath = model.PicturePath,
            Type = model.Type,
            PartMaterials = new List<PartMaterial>()

        }
        .SetCreateBySystem(now);
        foreach (var id in model.MaterialIds)
        {
            var material = await _dbContext.Materials.FirstOrDefaultAsync(x => x.Id == id);
            if (material == null)
            {
                ModelState
                    .AddModelError(nameof(model.MaterialIds), $"Material with id {id} not found");
            }
            newPart.PartMaterials.Add(new() { MaterialId = id, PartId = newPart.Id });
        }
        await _dbContext.AddAsync(newPart);
        await _dbContext.SaveChangesAsync();

        newPart = await _dbContext
            .Parts
            .FirstAsync(x => x.Id == newPart.Id);

        //create PartsMaterial in DB
        var newPartMaterial = newPart.PartMaterials.Select(partMaterial => new PartMaterial
        {
            Id = Guid.NewGuid(),
            PartId = partMaterial.PartId,
            MaterialId = partMaterial.MaterialId
        }).ToList();

        await _dbContext.AddRangeAsync(newPartMaterial);
        await _dbContext.SaveChangesAsync();

        var url = Url.Action(nameof(GetPartById), new { newPart.Id })
            ?? throw new Exception("failed to generate url");
        return Created(url, _mapper.ToDetail(newPart));
    }
    [HttpGet("api/v1/Part/")]
    public async Task<ActionResult<List<PartDetailModel>>> GetListPart(
        [FromQuery] PartFilter filter
        )
    {
        var dbEntities = await _dbContext
            .Set<Part>()
            .Include(x => x.PartMaterials)
              .ThenInclude(x => x.Material)
                .ThenInclude(x => x.TrashCanMaterials)
                    .ThenInclude(x => x.TrashCan)
            .FilterDeleted()
            .ApplyFilter(filter)
            .ToListAsync();

        var models = dbEntities.Select(_mapper.ToDetail).ToList();

        return Ok(models);
    }

    [HttpGet("api/v1/Part/{id:guid}")]
    public async Task<ActionResult<PartDetailModel>> GetPartById(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<Part>()
            .Include(x => x.PartMaterials)
            .ThenInclude(x => x.Material)
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        return Ok(_mapper.ToDetail(dbEntity));
    }

    [Authorize]
    [HttpPatch("api/v1/Part/{id:guid}")]
    public async Task<ActionResult<PartDetailModel>> UpdatePart(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<PartDetailModel> patch)
    {
        var dbEntity = await _dbContext
            .Set<Part>()
            .Include(x => x.PartMaterials)
            .ThenInclude(x => x.Material)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        var partToUpdate = _mapper.ToDetail(dbEntity);
        patch.ApplyTo(partToUpdate);
        if (!(ModelState.IsValid && TryValidateModel(partToUpdate)))
        {
            return ValidationProblem(ModelState);
        }
        dbEntity.Name = partToUpdate.Name;
        dbEntity.Description = partToUpdate.Description;
        dbEntity.IsVerified = partToUpdate.IsVerified;
        dbEntity.PicturePath = partToUpdate.PicturePath;

        var currentMaterials = dbEntity.PartMaterials;
        var updatedMaterials = partToUpdate.MaterialIds;
        var removedMaterials = currentMaterials.Where(x => !updatedMaterials.Any(y => y == x.PartId));
        var newMaterials = updatedMaterials.Where(x => !currentMaterials.Any(y => y.PartId == x));

        foreach (var material in removedMaterials)
        {
            dbEntity.PartMaterials.Remove(material);
        }
        foreach (var material in newMaterials)
        {
            dbEntity.PartMaterials.Add(new() { MaterialId = material });
        }

        dbEntity.SetModifyBySystem(_clock.GetCurrentInstant());
        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Part>()
            .Include(x => x.PartMaterials)
            .ThenInclude(x => x.Material)
            .FirstAsync(x => x.Id == id);

        return Ok(_mapper.ToDetail(dbEntity));
    }

    [Authorize]
    [HttpDelete("api/v1/Part/{id:guid}")]
    public async Task<IActionResult> DeletePart(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<Part>()
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
