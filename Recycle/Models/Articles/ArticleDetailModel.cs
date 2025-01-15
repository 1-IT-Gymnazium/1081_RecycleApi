using NodaTime.Text;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.Articles;

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
    public static ArticleDetailModel ToDetail(this Article source)
        => new()
        {
            Id = source.Id,
            Heading = source.Heading,
            Annotation = source.Annotation,
            AuthorsName = source.Author.UserName,
            Text = source.Text,
            PicturePath = source.PicturePath,
            CreatedAt = InstantPattern.ExtendedIso.Format(source.CreatedAt),
        };
}
