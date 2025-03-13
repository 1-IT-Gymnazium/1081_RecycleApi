using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MimeKit.Tnef;
using NodaTime;
using Recycle.Api.Models.Locations;
using Recycle.Api.Models.Materials;
using Recycle.Api.Models.Parts;
using Recycle.Api.Models.Products;
using Recycle.Api.Models.TrashCans;
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
    [Authorize]
    [HttpPost("api/v1/Material/")]
    public async Task<ActionResult> Create([FromBody] MaterialCreateModel model)
    {
        var checkMaterial = await _dbContext
            .Set<Material>()
            .AnyAsync(x => x.Name == model.Name);
        if (checkMaterial)
        {
            ModelState
                .AddModelError(nameof(model.Name), $"Material with this {model.Name} exists!");
            return ValidationProblem(ModelState);
        }

        var now = _clock.GetCurrentInstant();
        var newMaterial = new Material
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description,
            TrashCanMaterials = new List<TrashCanMaterial>()

        }
        .SetCreateBySystem(now);
        foreach (var id in model.TrashCanIds)
        {
            var trashCan = await _dbContext.TrashCans.FirstOrDefaultAsync(x => x.Id == id);
            if (trashCan == null)
            {
                ModelState
                    .AddModelError(nameof(model.TrashCanIds), $"Material with id {id} not found");
            }
            newMaterial.TrashCanMaterials.Add(new() { TrashCanId = id, MaterialId = newMaterial.Id });
        }
        await _dbContext.AddAsync(newMaterial);
        await _dbContext.SaveChangesAsync();

        newMaterial = await _dbContext
            .Materials
            .Include(x => x.TrashCanMaterials)
            .FirstAsync(x => x.Id == newMaterial.Id);

        var url = Url.Action(nameof(GetMaterialById), new { newMaterial.Id })
            ?? throw new Exception("failed to generate url");
        return Created(url, _mapper.ToDetail(newMaterial));
    }
    [HttpGet("api/v1/Material/")]
    public async Task<ActionResult<List<MaterialDetailModel>>> GetList()
    {
        var dbEntities = _dbContext
            .Materials
            .Include(x =>  x.TrashCanMaterials)
            .FilterDeleted()
            .Select(_mapper.ToDetail);

        return Ok(dbEntities);
    }
    [HttpGet("api/v1/Material/{id:guid}")]
    public async Task<ActionResult<MaterialDetailModel>> GetMaterialById(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<Material>()
            .Include(x => x.TrashCanMaterials)
            .ThenInclude(x => x.TrashCanId)
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        return Ok(_mapper.ToDetail(dbEntity));
    }
    [Authorize]
        [HttpPatch("api/v1/Material/{id:guid}")]
        public async Task<ActionResult<MaterialDetailModel>> UpdateMaterial(
            [FromRoute] Guid id,
            [FromBody] JsonPatchDocument<MaterialDetailModel> patch)
        {
        var dbEntity = await _dbContext
            .Set<Material>()
            .Include(x => x.TrashCanMaterials)
            .ThenInclude(x => x.TrashCan)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        var materialToUpdate = _mapper.ToDetail(dbEntity);
        patch.ApplyTo(materialToUpdate);
        if (!(ModelState.IsValid && TryValidateModel(materialToUpdate)))
        {
            return ValidationProblem(ModelState);
        }
        dbEntity.Name = materialToUpdate.Name;
        dbEntity.Description = materialToUpdate.Description;

        var currentTrashCans = dbEntity.TrashCanMaterials;
        var updatedTrashCans = materialToUpdate.TrashCanIds;
        var removedTrashCans = currentTrashCans.Where(x => !updatedTrashCans.Any(y => y == x.MaterialId));
        var newTrashCans = updatedTrashCans.Where(x => !currentTrashCans.Any(y => y.MaterialId == x));

        foreach (var trashCan in removedTrashCans)
        {
            dbEntity.TrashCanMaterials.Remove(trashCan);
        }
        foreach (var trashCan in newTrashCans)
        {
            dbEntity.TrashCanMaterials.Add(new() { TrashCanId = trashCan });
        }

        dbEntity.SetModifyBySystem(_clock.GetCurrentInstant());
        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Material>()
            .Include(x => x.TrashCanMaterials)
            .ThenInclude(x => x.TrashCan)
            .FirstAsync(x => x.Id == id);

        return Ok(_mapper.ToDetail(dbEntity));
        }

    [Authorize]
        [HttpDelete("api/v1/Material/{id:guid}")]
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

