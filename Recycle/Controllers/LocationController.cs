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
[ApiController]
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
    public async Task<ActionResult<LocationDetailModel>> CreateLocation(
    [FromBody] LocationCreateModel model)
    {
    var checkLocation = await _dbContext
    .Set<Location>()
    .AnyAsync(x => x.Name == model.Name);
        if (checkLocation)
        {
            ModelState
                   .AddModelError(nameof(model.Name), $"Location with the name of {model.Name} already exists!");
            return ValidationProblem(ModelState);
        }

        var now = _clock.GetCurrentInstant();
        var newLocation = new Location
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
        }
        .SetCreateBySystem(now);

        _dbContext.Add(newLocation);
        await _dbContext.SaveChangesAsync();

        newLocation = await _dbContext
            .Locations
            .FirstAsync(x => x.Id == newLocation.Id);

        var url = Url.Action(nameof(GetListLocations), new { newLocation.Id })
            ?? throw new Exception();
        return Created(url, _mapper.ToDetail(newLocation));
    }
    [HttpGet("api/v1/Location/")]
    public async Task<ActionResult<List<LocationDetailModel>>> GetListLocations()
    {
        var dbEntities = _dbContext
            .Locations
            .FilterDeleted()
            .ToList();
        return Ok(dbEntities);
    }
    [HttpGet("api/v1/Location/{id:guid}")]
    public async Task<ActionResult<LocationDetailModel>> GetById(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<Location>()
            .FilterDeleted()
            .FirstOrDefaultAsync();
        if (dbEntity == null)
        {
            return NotFound();
        }
        var location = new LocationDetailModel
        {
            Id = dbEntity.Id,
            Name = dbEntity.Name,
        };
        return Ok(location);
    }
    [HttpPatch("api/v1/Location/{id:guid}")]
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

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Location>()
            .FirstOrDefaultAsync(x => x.Id == id);
        return Ok(_mapper.ToDetail(dbEntity));
    }

    [HttpDelete("api/v1/Location/{id:guid}")]
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
