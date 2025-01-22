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
            Type = (PartType)model.Type 
        }.SetCreateBySystem(now);

        _dbContext.Add(newPart);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }
    [HttpGet("api/v1/Part/")]
    public async Task<ActionResult<List<PartDetailModel>>> GetListPart()
    {
        var dbEntities = _dbContext
            .Parts
            .FilterDeleted()
            .ToList();
        return Ok(dbEntities);
    }
    [HttpGet("api/v1/Part/{id:guid}")]
    public async Task<ActionResult<PartDetailModel>> GetById(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<Part>()
            .FilterDeleted()
            .FirstOrDefaultAsync();
        if (dbEntity == null)
        {
            return NotFound();
        };
        var part = new PartDetailModel
        {
            Id = dbEntity.Id,
            Name = dbEntity.Name,
            IsVerified = dbEntity.IsVerified,
        };
        return Ok(part);
    }

    [Authorize]
    [HttpPatch("api/v1/Part/{id:guid}")]
    public async Task<ActionResult<PartDetailModel>> UpdatePart(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<PartDetailModel> patch)
    {
        var dbEntity = await _dbContext
            .Set<Part>()
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

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Part>()
            .FirstOrDefaultAsync(x => x.Id == id);

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
