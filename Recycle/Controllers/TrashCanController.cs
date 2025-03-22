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
/// <summary>
/// Controller responsible for managing trash cans in the system, 
/// including creation, listing, image uploads, updating and soft deletion.
/// </summary>
[ApiController]
public class TrashCanController : ControllerBase
{
    private readonly ILogger<TrashCanController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;
    private readonly IApplicationMapper _mapper;
    private readonly IImageService _imageService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrashCanController"/> class with required services.
    /// </summary>
    public TrashCanController(ILogger<TrashCanController> logger, IClock clock, AppDbContext dbContext, IApplicationMapper mapper, IImageService imageService)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
        _mapper = mapper;
        _imageService = imageService;
    }

    /// <summary>
    /// Creates a new trash can in the database.
    /// </summary>
    /// <param name="model">Model containing trash can name, type, image, and description.</param>
    /// <returns>
    /// Returns 201 (Created) with the created trash can detail, or 
    /// 400 (Bad Request) if a trash can with the same name already exists.
    /// </returns>
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
            PicturePath = model.PicturePath,
            Description = model.Description,
        }
        .SetCreateBySystem(now);

        await _dbContext.AddAsync(newTrashCan);
        await _dbContext.SaveChangesAsync();

        newTrashCan = await _dbContext
            .TrashCans
            .FirstAsync(x => x.Id == newTrashCan.Id);

        var url = Url.Action(nameof(GetById), new { newTrashCan.Id })
            ?? throw new Exception("failed to generate url");
        return Created(url, _mapper.ToDetail(newTrashCan));
    }

    /// <summary>
    /// Uploads an image for a trash can and stores it using the image service.
    /// </summary>
    /// <param name="trashCanImage">Uploaded image file.</param>
    /// <returns>
    /// Returns 200 (OK) with the saved image path, or 400 (Bad Request) if no file is uploaded.
    /// </returns>
    [HttpPost("api/v1/TrashCan/UploadTrashCanImage")]
    public async Task<IActionResult> UploadContainerImage(IFormFile trashCanImage)
    {
        if (trashCanImage == null || trashCanImage.Length == 0)
        {
            return BadRequest(new { error = "NO_FILE_UPLOADED", message = "No container image uploaded." });
        }

        // Save the image using the ImageService
        var newImagePath = await _imageService.SaveImageAsync(trashCanImage, "TrashCanImages");

        // Return the stored image path
        return Ok(new { message = "Container image uploaded successfully.", imagePath = newImagePath });
    }

    /// <summary>
    /// Retrieves a list of all non-deleted trash cans.
    /// </summary>
    /// <returns>
    /// Returns 200 (OK) with a list of trash can details.
    /// </returns>
    [HttpGet("api/v1/TrashCan/")]
    public async Task<ActionResult<List<TrashCanDetailModel>>> GetListTrashCan()
    {
        var dbEntities = _dbContext
            .TrashCans
            .FilterDeleted()
            .Select(_mapper.ToDetail);
        return Ok(dbEntities);
    }

    /// <summary>
    /// Retrieves a specific trash can by its ID.
    /// </summary>
    /// <param name="id">The unique identifier of the trash can.</param>
    /// <returns>
    /// Returns 200 (OK) with trash can details, or 404 (Not Found) if not found.
    /// </returns>
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
        return Ok(_mapper.ToDetail(dbEntity));
    }

    /// <summary>
    /// Updates an existing trash can using a JSON patch document.
    /// </summary>
    /// <param name="id">The ID of the trash can to update.</param>
    /// <param name="patch">Patch document containing updated fields.</param>
    /// <returns>
    /// Returns 200 (OK) with updated details, 404 (Not Found) if trash can does not exist, 
    /// or 400 (Bad Request) if validation fails.
    /// </returns>
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

    /// <summary>
    /// Soft deletes a trash can by marking it as deleted in the system.
    /// </summary>
    /// <param name="id">The ID of the trash can to delete.</param>
    /// <returns>
    /// Returns 204 (No Content) if successfully deleted, or 404 (Not Found) if not found.
    /// </returns>
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
