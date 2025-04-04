using Recycle.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Articles;

/// <summary>
/// Represents the data required to create a new article, including basic metadata like heading, author name, annotation, content and image path.
/// </summary>
public class ArticleCreateModel
{
    [Required(ErrorMessage = "{0} is required.", AllowEmptyStrings = false)]
    public Guid Id { get; set; }
    public string Heading { get; set; } = null!;
    public string AuthorsName { get; set; } = null!;
    public string? Annotation { get; set; }
    public string? Text { get; set; }
    public string PicturePath { get; set; }
    }
public static class ArticleCreateModelExtensions
{
    /// <summary>
    /// Provides mapping extensions for converting article entities into model objects.
    /// </summary>
    public static ArticleCreateModel ToUpdate(this Article source) => new()
    {
        Heading = source.Heading,
        Annotation = source.Annotation,
        PicturePath = source.PicturePath,
        Text = source.Text,
    };
}
