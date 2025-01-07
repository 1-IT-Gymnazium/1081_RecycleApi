using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.TrashCans;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Recycle.Api.Controllers;
[ApiController]
public class TrashCanController : ControllerBase
{
    private readonly ILogger<TrashCanController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;
    private readonly IApplicationMapper _mapper;

    public TrashCanController(ILogger<TrashCanController> logger, IClock clock, AppDbContext dbContext, IApplicationMapper mapper)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
        _mapper = mapper;
    }
    [HttpPost("api/v1/TrashCan/")]
    public async Task<ActionResult> CreateTrashCan(
        [FromBody] TrashCanCreateModel model)
    {
    var checkTrashCan = await _dbContext
    .Set<TrashCan>()
    .AnyAsync(x => x.Name == model.Name);
        if (checkTrashCan)
        {
            ModelState
                   .AddModelError(nameof(model.Name), $"Part with the name of {model.Name} already exists!");
            return ValidationProblem(ModelState);
        }

        var now = _clock.GetCurrentInstant();
        var newTrashCan = new TrashCan
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Type = model.Type,
            Description = model.Description,
            PicturePath = model.PicturePath,
        }
        .SetCreateBySystem(now);

        _dbContext.Add(newTrashCan);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    [HttpGet("api/v1/TrashCan/")]
    public async Task<ActionResult<List<TrashCanDetailModel>>> GetListTrashCan()
    {
        var dbEntities = _dbContext
            .TrashCans
            .Select(_mapper.ToDetail);
        return Ok(dbEntities);
    }
    [HttpGet("api/v1/TrashCan/{id:guid}")]
    public async Task<ActionResult<TrashCanDetailModel>> GetById(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<TrashCan>()
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        var trashCan = new TrashCanDetailModel
        {
            Id = dbEntity.Id,
            Name = dbEntity.Name,
            PicturePath = dbEntity.PicturePath,
        };
        return Ok(trashCan);
    }
    [HttpPatch("api/v1/TrashCan/{id:guid}")]
    public async Task<ActionResult<TrashCanDetailModel>> UpdateTrashCan(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<TrashCanDetailModel> patch
        )
    {
        var dbEntity = await _dbContext
            .Set<TrashCan>()
            .FirstOrDefaultAsync( x => x.Id == id);
        if(dbEntity == null)
        {
            return NotFound();
        }

        var trashCanToUpdate = _mapper.ToDetail(dbEntity);
        patch.ApplyTo(trashCanToUpdate);

        var uniqueCheck = await _dbContext
            .Set<TrashCan>()
            .AnyAsync( x => x.Id != id && x.Name == trashCanToUpdate.Name);
        if (uniqueCheck)
        {
            ModelState.AddModelError<TrashCanDetailModel>(x => x.Name, "Name already used, try different");
        }
        if(!(ModelState.IsValid && TryValidateModel(trashCanToUpdate)))
        {
            return ValidationProblem(ModelState);
        }
        dbEntity.Name = trashCanToUpdate.Name;
        dbEntity.Type = trashCanToUpdate.Type;
        dbEntity.Description = trashCanToUpdate.Description;
        dbEntity.PicturePath = trashCanToUpdate.PicturePath;

        await _dbContext .SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<TrashCan>()
            .FirstOrDefaultAsync ( x => x.Id == id);
        return Ok(_mapper.ToDetail(dbEntity));
    }
    [HttpDelete("api/v1/TrashCan/{id:guid}")]
    public async Task<IActionResult> DeleteTrashCan(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<TrashCan>()
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
