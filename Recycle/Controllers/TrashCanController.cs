using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NodaTime;
using Recycle.Api.Models.TrashCans;
using Recycle.Api.Services;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Recycle.Data.Entities.TrashCan;

namespace Recycle.Api.Controllers;
[ApiController]
public class TrashCanController : ControllerBase
{
    private readonly ILogger<TrashCanController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;
    private readonly IApplicationMapper _mapper;
    private readonly IImageService _imageService;

    public TrashCanController(ILogger<TrashCanController> logger, IClock clock, AppDbContext dbContext, IApplicationMapper mapper, IImageService imageService)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
        _mapper = mapper;
        _imageService = imageService;
    }
    [Authorize]
    [HttpPost("api/v1/TrashCan")]
    public async Task<ActionResult> CreateTrashCan(
        [FromBody] TrashCanCreateModel model)
    {
        var checkTrashCan = await _dbContext
        .Set<TrashCan>()
        .FilterDeleted()
        .AnyAsync(x => x.Name == model.Name);
        if (checkTrashCan)
        {
            ModelState
                   .AddModelError(nameof(model.Name), $"Trash Can with the name of {model.Name} already exists!");
            return ValidationProblem(ModelState);
        }

        var now = _clock.GetCurrentInstant();
        var newTrashCan = new TrashCan
        {
            Id = Guid.NewGuid(),
            Name = model.Name,
            Type = model.Type,
            Description = model.Description,
        }
        .SetCreateBySystem(now);

        _dbContext.Add(newTrashCan);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
    //fixni tohle more
    /*
    [HttpPost("api/v1/TrashCan/UploadTrashcanImage/{trashcanId:guid}")]
    public async Task<IActionResult> UploadTrashcanImage([FromRoute] Guid trashcanId, IFormFile trashcanImage)
    {
        var trashcan = await _dbContext.TrashCans.FindAsync(trashcanId);
        if (trashcan == null)
        {
            return NotFound(new { error = "TRASHCAN_NOT_FOUND", message = "Trashcan not found." });
        }

        if (trashcanImage == null || trashcanImage.Length == 0)
        {
            return BadRequest(new { error = "NO_FILE_UPLOADED", message = "No trashcan image uploaded." });
        }

        // Save new trashcan image
        var newImagePath = await _imageService.SaveImageAsync(trashcanImage, "TrashcanImages");

        // Delete old image if it exists
        if (!string.IsNullOrEmpty(trashcan.PicturePath))
        {
            await _imageService.DeleteImageAsync(trashcan.PicturePath);
        }

        // Update trashcan image path
        trashcan.PicturePath = newImagePath;
        _dbContext.TrashCans.Update(trashcan);
        await _dbContext.SaveChangesAsync();

        return Ok(new { message = "Trashcan image uploaded successfully.", imagePath = newImagePath });
    }
    */

    [HttpGet("api/v1/TrashCan/")]
    public async Task<ActionResult<List<TrashCanDetailModel>>> GetListTrashCan()
    {
        var dbEntities = _dbContext
            .TrashCans
            .FilterDeleted()
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
            Type = dbEntity.Type,
            Description = dbEntity.Description,
            PicturePath = dbEntity.PicturePath,
        };
        return Ok(trashCan);
    }
    [Authorize]
    [HttpPatch("api/v1/TrashCan/{id:guid}")]
    public async Task<ActionResult<TrashCanDetailModel>> UpdateTrashCan(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<TrashCanDetailModel> patch
        )
    {
        var dbEntity = await _dbContext
            .Set<TrashCan>()
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }

        var trashCanToUpdate = _mapper.ToDetail(dbEntity);
        patch.ApplyTo(trashCanToUpdate);

        var uniqueCheck = await _dbContext
            .Set<TrashCan>()
            .AnyAsync(x => x.Id != id && x.Name == trashCanToUpdate.Name);
        if (uniqueCheck)
        {
            ModelState.AddModelError<TrashCanDetailModel>(x => x.Name, "Name already used, try different");
        }
        if (!(ModelState.IsValid && TryValidateModel(trashCanToUpdate)))
        {
            return ValidationProblem(ModelState);
        }
        dbEntity.Name = trashCanToUpdate.Name;
        dbEntity.Type = trashCanToUpdate.Type;
        dbEntity.Description = trashCanToUpdate.Description;
        dbEntity.PicturePath = trashCanToUpdate.PicturePath;

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<TrashCan>()
            .FirstOrDefaultAsync(x => x.Id == id);
        return Ok(_mapper.ToDetail(dbEntity));
    }
    [Authorize]
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
