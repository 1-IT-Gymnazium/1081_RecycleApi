using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Locations;
using Recycle.Api.Models.TrashCans;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;

namespace Recycle.Api.Controllers;

public class LocationController : ControllerBase
{
    private readonly ILogger<LocationController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;
    private readonly IApplicationMapper _mapper;

    public LocationController(ILogger<LocationController> logger, IClock clock, AppDbContext dbContext, IApplicationMapper mapper)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
        _mapper = mapper;
    }
    [HttpPost("api/v1/Location/")]
    public async Task<ActionResult> CreateTrashCan(
    [FromBody] LocationCreateModel model)
    {
        var now = _clock.GetCurrentInstant();
        var newTrashCan = new Location
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Region = model.Region,
        }
        .SetCreateBySystem(now);

        _dbContext.Add(newTrashCan);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }
    [HttpGet("api/v1/Location/")]
    public async Task<ActionResult<List<LocationDetailModel>>> GetListLocations()
    {
        var dbEntities = _dbContext.TrashCans.ToList();
        return Ok(dbEntities);
    }
    [HttpGet("api/v1/Location{Id:Guid}")]
    public async Task<ActionResult<LocationDetailModel>> UpdateLocation(
    [FromRoute] Guid id,
    [FromBody] JsonPatchDocument<LocationDetailModel> patch
    )
    {
        var dbEntity = await _dbContext
            .Set<Location>()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }

        var locationToUpdate = _mapper.ToDetail(dbEntity);
        patch.ApplyTo(locationToUpdate);

        var uniqueCheck = await _dbContext
            .Set<Location>()
            .AnyAsync(x => x.Id != id && x.Name == locationToUpdate.Name);
        if (uniqueCheck)
        {
            ModelState.AddModelError<LocationDetailModel>(x => x.Name, "Name already used, it might be already crated or try different");
        }
        if (!(ModelState.IsValid && TryValidateModel(locationToUpdate)))
        {
            return ValidationProblem(ModelState);
        }
        dbEntity.Name = locationToUpdate.Name;
        dbEntity.Region = locationToUpdate.Region;

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Location>()
            .FirstOrDefaultAsync(x => x.Id == id);
        return Ok(_mapper.ToDetail(dbEntity));
    }
    public async Task<IActionResult> DeleteMaterial(
    [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<Location>()
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
