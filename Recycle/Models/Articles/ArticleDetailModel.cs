using NodaTime.Text;
using Recycle.Api.Utilities;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.Articles;

/// <summary>
/// Represents full article details returned to clients, including content, author info, and creation date.
/// </summary>
public class ArticleDetailModel
{
    public Guid Id { get; set; }
    public string Heading { get; set; } = null!;
    public string Annotation { get; set; } = null!;
    public string AuthorsName { get; set; } = null!;
    public string? Text { get; set;}
    public string? PicturePath { get; set; }
    public string CreatedAt { get; set; } = null!;
}
public static class ArticleDetailModelExtensions
{
    /// <summary>
    /// Provides a mapping method to convert an <see cref="Article"/> entity into a detailed view model.
    /// </summary>
    public static ArticleDetailModel ToDetail(this IApplicationMapper mapper, Article source)
        => new()
        {
            Id = source.Id,
            Heading = source.Heading,
            Annotation = source.Annotation,
            AuthorsName = source.Author.UserName ?? string.Empty,
            Text = source.Text,
            PicturePath = string.IsNullOrEmpty(source.PicturePath) ? null : $"{mapper.EnviromentSettings.BackendHostUrl}{source.PicturePath}",
            CreatedAt = InstantPattern.ExtendedIso.Format(source.CreatedAt),
        };
}
