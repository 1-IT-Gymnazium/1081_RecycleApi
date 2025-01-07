using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Articles;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;

namespace Recycle.Api.Controllers;

[ApiController]
public class ArticleController : ControllerBase
{
    private readonly ILogger<ArticleController> _logger;
    private readonly IClock _clock;
    private readonly AppDbContext _dbContext;

    //TODO: Refactor
    public ArticleController(
        ILogger<ArticleController> logger,
        IClock clock,
        AppDbContext dbContext)
    {
        _logger = logger;
        _clock = clock;
        _dbContext = dbContext;
    }
    //[Authorize]
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
        return Created(url, newArticle.ToDetail());
    }

    [HttpGet("api/v1/Article/")]
    public async Task<ActionResult<List<ArticleDetailModel>>> GetList()
    {
        var dbEntities = await _dbContext
            .Articles
            .Include(x => x.Author)
            .FilterDeleted()
            .ToListAsync();

        return Ok(dbEntities.Select(x => x.ToDetail()));
    }
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

        var result = new ArticleDetailModel
        {
            Id = dbEntity.Id,
            Heading = dbEntity.Heading,
            Annotation = dbEntity.Annotation,
        };
        return Ok(result);
    }
    // more oprtions to build update 
    // [HttpPut]
    //[Authorize]
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
        var toUpdate = dbEntity.ToDetail();
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
        dbEntity.PicturePath = toUpdate.PicturePath;
        dbEntity.SetModifyBySystem(_clock.GetCurrentInstant());

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Article>()
            .FirstAsync(x => x.Id == id);
        return Ok(dbEntity.ToDetail());
    }
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
