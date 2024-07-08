using NodaTime.Text;
using RecycleApp.Data.Entities;

namespace Recycle.Api.Controllers.Models.Articles
{
    public class ArticleDetailModel
    {
        public Guid Id { get; set; }
        public string Heading { get; set; } = null!;
        public string Annotation { get; set; } = null!;
        public string AuthorsName { get; set; } = null!;
        public string DateOfCreation { get; set; } = null!;
    }
    public static class ArticleDetailModelExtensions
    {
        public static ArticleDetailModel ToDetail(this Article source)
            => new()
            {
                Id = source.Id,
                Heading = source.Heading,
                Annotation = source.Annotation,
                AuthorsName = source.AuthorsName,
                DateOfCreation = InstantPattern.ExtendedIso.Format(source.DateOfCreation),
            };
    }

}
