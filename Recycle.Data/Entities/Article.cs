using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecycleApp.Data.Entities
{
    public class Article
    {
        public Guid Id { get; set; }
        public string Heading { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string AuthorsName { get; set; } = null!;
        public Instant DateOfCreation { get; set; }
        public string Annotation { get; set; } = null!;
        public Instant? DeletedAt { get; set; }

        public void SetDeleteBySystem(Instant instant)
        {
            throw new NotImplementedException();
        }
    }
    public static class ArticleExtensions
    {
        public static IQueryable<Article> FilterDeleted(this IQueryable<Article> query)
            => query
            .Where(x => x.DeletedAt == null)
            ;
    }

}
