using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Articles;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Interfaces;

namespace Recycle.Api.Controllers;

[ApiController]
[Route("[controller]")]
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
    [HttpGet("api/v1/Article")]
    public async Task<ActionResult<List<ArticleDetailModel>>> GetList()
    {
        var dbEntities = await _dbContext
            .Set<Article>()
            .FilterDeleted()
            .ToListAsync();

        return Ok(dbEntities.Select(x => x.ToDetail()));
    }
    [HttpGet("api/v1/Article{id:guid}")]
    public async Task<ActionResult<ArticleDetailModel>> Get(
        [FromRoute] Guid id
        )
    {
        var dbEntity = await _dbContext
            .Set<Article>()
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
    [HttpPost("api/v1/Article")]
    public async Task<ActionResult> Create(
        [FromRoute] ArticleCreateModel model
        )
    {
        var now = _clock.GetCurrentInstant();
        var newArticle = new Article
        {
            Id = Guid.NewGuid(),
            Heading = model.Heading,
            Annotation = model.Annotation,
        };
        //var uniqueCheck

        _dbContext.Add(newArticle);
        await _dbContext.SaveChangesAsync();

        var dbEntity = await _dbContext
            .Set<Article>()
            .FirstAsync(x => x.Id == newArticle.Id);
        var url = Url.Action(nameof(Get), new { dbEntity.Id }) ??
        throw new Exception("Failed to generate url");
        return Created(url, dbEntity.ToDetail());
    }
    // more oprtions to build update 
    // [HttpPut]
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

        await _dbContext.SaveChangesAsync();

        dbEntity = await _dbContext
            .Set<Article>()
            .FirstAsync(x => x.Id == id);
        return Ok(dbEntity.ToDetail());
    }
    [HttpDelete("api/v1/Article/{id}")]
    public async Task<ActionResult> Delete(
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
