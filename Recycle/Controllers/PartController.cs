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

/// <summary>
/// Controller responsible for managing parts and their relationships to materials.
/// Includes CRUD operations and filtering support.
/// </summary>
[ApiController]
public class PartController : ControllerBase
{
    private readonly ILogger<ProductController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;
    private readonly IApplicationMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PartController"/> class with required services.
    /// </summary>
    public PartController(ILogger<ProductController> logger, IClock clock, AppDbContext dbContext, IApplicationMapper mapper)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new part and assigns it to a material.
    /// </summary>
    /// <param name="model">Model containing part name, description, type, material ID, and image.</param>
    /// <returns>
    /// Returns 201 (Created) with the part detail, or 400 (Bad Request) if the part already exists or material is invalid.
    /// </returns>
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
        var checkMaterial = await _dbContext
            .Set<Material>()
            .AnyAsync(x => x.Id == model.MaterialId);
        if (checkMaterial == null)
        {
            ModelState
                   .AddModelError(nameof(model.MaterialId), $"Part with the name of {model.Name} already exists!");
            return ValidationProblem(ModelState);
        }

        var now = _clock.GetCurrentInstant();
        var newPart = new Part
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Description = model.Description,
            PicturePath = model.PicturePath,
            Type = model.Type,
            PartMaterialId = model.MaterialId,
        }
        .SetCreateBySystem(now);

        await _dbContext.AddAsync(newPart);
        await _dbContext.SaveChangesAsync();

        newPart = await _dbContext
            .Parts
            .Include(x => x.PartMaterial)
            .FirstAsync(x => x.Id == newPart.Id);

        await _dbContext.SaveChangesAsync();

        var url = Url.Action(nameof(GetPartById), new { newPart.Id })
            ?? throw new Exception("failed to generate url");
        return Created(url, _mapper.ToDetail(newPart));
    }

    /// <summary>
    /// Retrieves a list of parts with optional filters applied.
    /// Includes material and trash can associations.
    /// </summary>
    /// <param name="filter">Filtering options for the parts list.</param>
    /// <returns>
    /// Returns 200 (OK) with a list of parts matching the filter.
    /// </returns>
    [HttpGet("api/v1/Part/")]
    public async Task<ActionResult<List<PartDetailModel>>> GetListPart(
        [FromQuery] PartFilter filter
        )
    {
        var dbEntities = await _dbContext
            .Set<Part>()
            .Include(x => x.PartMaterial)
                .ThenInclude(x => x.TrashCanMaterials)
                    .ThenInclude(x => x.TrashCan)
            .FilterDeleted()
            .ApplyFilter(filter)
            .ToListAsync();

        var models = dbEntities.Select(_mapper.ToDetail).ToList();

        return Ok(models);
    }

    /// <summary>
    /// Retrieves a specific part by its ID including related material.
    /// </summary>
    /// <param name="id">The ID of the part.</param>
    /// <returns>
    /// Returns 200 (OK) with part detail, or 404 (Not Found) if not found.
    /// </returns>
    [HttpGet("api/v1/Part/{id:guid}")]
    public async Task<ActionResult<PartDetailModel>> GetPartById(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<Part>()
            .Include(x => x.PartMaterial)
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        return Ok(_mapper.ToDetail(dbEntity));
    }

    /// <summary>
    /// Updates a part's properties using a JSON Patch document.
    /// Also allows reassigning the material if valid.
    /// </summary>
    /// <param name="id">The ID of the part to update.</param>
    /// <param name="patch">Patch document containing updated fields.</param>
    /// <returns>
    /// Returns 200 (OK) with the updated part detail, 
    /// or 404 (Not Found) if part does not exist, 
    /// or 400 (Bad Request) if validation fails.
    /// </returns>
    [Authorize]
    [HttpPatch("api/v1/Part/{id:guid}")]
    public async Task<ActionResult<PartUpdateModel>> UpdatePart(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<PartUpdateModel> patch)
    {
        var dbEntity = await _dbContext
            .Set<Part>()
            .Include(x => x.PartMaterial)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        var partToUpdate = _mapper.ToUpdate(dbEntity);
        patch.ApplyTo(partToUpdate);
        if (!(ModelState.IsValid && TryValidateModel(partToUpdate)))
        {
            return ValidationProblem(ModelState);
        }
        dbEntity.Name = partToUpdate.Name;
        dbEntity.Description = partToUpdate.Description;
        dbEntity.IsVerified = partToUpdate.IsVerified;
        dbEntity.PicturePath = partToUpdate.PicturePath;
        if (partToUpdate.MaterialId != dbEntity.PartMaterialId)
        {
            var materialExists = await _dbContext
                .Set<Material>()
                .AnyAsync(m => m.Id == partToUpdate.MaterialId);

            if (!materialExists)
            {
                ModelState
                    .AddModelError(nameof(partToUpdate.MaterialId), "The specified Material does not exist.");
                return ValidationProblem(ModelState);
            }

            dbEntity.PartMaterialId = partToUpdate.MaterialId;
        }
        dbEntity.SetModifyBySystem(_clock.GetCurrentInstant());
        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Part>()
            .Include(x => x.PartMaterial)
            .FirstAsync(x => x.Id == id);

        return Ok(_mapper.ToDetail(dbEntity));
    }

    /// <summary>
    /// Deletes a part by marking it as deleted (soft delete).
    /// </summary>
    /// <param name="id">The ID of the part to delete.</param>
    /// <returns>
    /// Returns 204 (No Content) if successful, or 404 (Not Found) if the part is not found.
    /// </returns>
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
