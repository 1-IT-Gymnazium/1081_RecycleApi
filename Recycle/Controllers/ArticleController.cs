using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Articles;
using Recycle.Api.Services;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;

namespace Recycle.Api.Controllers;
/// <summary>
/// Initializes a new instance of the <see cref="ArticleController"/> class with required services.
/// </summary>
[ApiController]
public class ArticleController : ControllerBase
{
    private readonly ILogger<ArticleController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;
    private readonly IImageService _imageService;
    private readonly IApplicationMapper _mapper;

    //TODO: Refactor
    public ArticleController(
        ILogger<ArticleController> logger,
        IClock clock,
        AppDbContext dbContext,
        IImageService imageService,
        IApplicationMapper mapper
        )
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
        _imageService = imageService;
        _mapper = mapper;
    }
    /// <summary>
    /// Creates a new article by checking if the article heading already exists, 
    /// and then saving the new article to the database.
    /// </summary>
    /// <param name="model">The details of the article to be created, including heading, annotation, and picture path.</param>
    /// <returns>
    /// Returns 201 (Created) with the URL to the new article if successful, 
    /// or 400 (Bad Request) with an error if the article with the same heading already exists.
    /// </returns>

    [Authorize]
    [HttpPost("api/v1/Article/")]
    public async Task<ActionResult> Create(
        [FromBody] ArticleCreateModel model
        )
    {
        var checkArticle =
            await _dbContext
            .Set<Article>()
            .AnyAsync(x => x.Heading == model.Heading);
        if (checkArticle)
        {
            ModelState
                   .AddModelError(nameof(model.Heading), $"Article with the name of {model.Heading} already exists!");
            return ValidationProblem(ModelState);
        }

        var now = _clock.GetCurrentInstant();
        var newArticle = new Article
        {
            Id = Guid.NewGuid(),
            Heading = model.Heading,
            Annotation = model.Annotation,
            Text = model.Text,
            AuthorId = User.GetUserId(),
            PicturePath = model.PicturePath,
        }
        .SetCreateBySystem(now);

        await _dbContext.AddAsync(newArticle);
        await _dbContext.SaveChangesAsync();

        newArticle = await _dbContext
           .Articles
           .Include(x => x.Author)
           .FirstAsync(x => x.Id == newArticle.Id);
        var url = Url.Action(nameof(Get), new { newArticle.Id }) ??
        throw new Exception("Failed to generate url");
        return Created(url, _mapper.ToDetail(newArticle));
    }

    /// <summary>
    /// Uploads an image for an article and stores it using the image service.
    /// </summary>
    /// <param name="articleImage">The image file to upload.</param>
    /// <returns>
    /// Returns 200 (OK) with the saved image path if successful, 
    /// or 400 (Bad Request) if no file was uploaded.
    /// </returns>
    [HttpPost("api/v1/Article/UploadArticleImage")]
    public async Task<IActionResult> UploadArticleImage(IFormFile articleImage)
    {
        if (articleImage == null || articleImage.Length == 0)
        {
            return BadRequest(new { error = "NO_FILE_UPLOADED", message = "No article image uploaded." });
        }

        // Save the image using the ImageService
        var newImagePath = await _imageService.SaveImageAsync(articleImage, "ArticleImages");

        // Return the stored image path
        return Ok(new { message = "Article image uploaded successfully.", imagePath = newImagePath });
    }

    /// <summary>
    /// Retrieves a list of all articles, including their author information, 
    /// and filters out deleted articles.
    /// </summary>
    /// <returns>
    /// Returns 200 (OK) with a list of article details.
    /// </returns>

    [HttpGet("api/v1/Article/")]
    public async Task<ActionResult<List<ArticleDetailModel>>> GetList()
    {
        var dbEntities = ( await _dbContext
            .Articles
            .Include(x => x.Author)
            .FilterDeleted()
            .ToListAsync())
            .Select(_mapper.ToDetail);

        return Ok(dbEntities);
    }

    /// <summary>
    /// Retrieves the details of a specific article by its ID, including author information, 
    /// and ensures that the article is not deleted.
    /// </summary>
    /// <param name="id">The ID of the article to retrieve.</param>
    /// <returns>
    /// Returns 200 (OK) with the article details if found, 
    /// or 404 (Not Found) if the article does not exist.
    /// </returns>
    [HttpGet("api/v1/Article/{id:guid}")]
    public async Task<ActionResult<ArticleDetailModel>> Get(
        [FromRoute] Guid id
        )
    {
        var dbEntity = await _dbContext
            .Articles
            .Include(x => x.Author)
            .FilterDeleted()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }

        return Ok(_mapper.ToDetail(dbEntity));
    }

    /// <summary>
    /// Updates an existing article's details (heading, annotation, picture path) by applying a patch.
    /// It also checks if the updated heading is unique before saving the changes.
    /// </summary>
    /// <param name="id">The ID of the article to update.</param>
    /// <param name="patch">The patch document containing the fields to update.</param>
    /// <returns>
    /// Returns 200 (OK) with the updated article details if successful, 
    /// or 404 (Not Found) if the article doesn't exist, 
    /// or 400 (Bad Request) if there are validation errors.
    /// </returns>

    // more oprtions to build update for example [HttpPut]
    [Authorize]
    [HttpPatch("api/v1/Article/{id:guid}")]
    public async Task<ActionResult<ArticleDetailModel>> Update(
        [FromRoute] Guid id,
        [FromBody] JsonPatchDocument<ArticleDetailModel> patch)

    {
        var dbEntity = await _dbContext
            .Set<Article>()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (dbEntity == null)
        {
            return NotFound();
        }
        var toUpdate = _mapper.ToDetail(dbEntity);
        patch.ApplyTo(toUpdate);

        var uniqueCheck = await _dbContext
            .Set<Article>()
            .AnyAsync(x => x.Heading == toUpdate.Heading);

        if (uniqueCheck)
        {
            ModelState.AddModelError<ArticleDetailModel>(x => x.Heading, "Heading is not unique");
        }

        if (!(ModelState.IsValid && TryValidateModel(toUpdate)))
        {
            return ValidationProblem(ModelState);
        }

        dbEntity.Heading = toUpdate.Heading;
        dbEntity.Annotation = toUpdate.Annotation;
        dbEntity.Text = toUpdate.Text;
        dbEntity.PicturePath = toUpdate.PicturePath;
        dbEntity.SetModifyBySystem(_clock.GetCurrentInstant());

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Article>()
            .FirstAsync(x => x.Id == id);
        return Ok(_mapper.ToDetail(dbEntity));
    }

    /// <summary>
    /// Deletes an article by setting it as deleted in the database (soft delete).
    /// </summary>
    /// <param name="id">The ID of the article to delete.</param>
    /// <returns>
    /// Returns 204 (No Content) if the article is successfully deleted, 
    /// or 404 (Not Found) if the article does not exist.
    /// </returns>
    [Authorize]
    [HttpDelete("api/v1/Article/{id:guid}")]
    // For empty result always use Interface.(IActionResult)
    public async Task<IActionResult> DeleteArticle(
        [FromRoute] Guid id)
    {
        var dbEntity = await _dbContext
            .Set<Article>()
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
