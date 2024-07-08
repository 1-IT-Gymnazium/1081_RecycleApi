using RecycleApp.Data.Entities;


namespace Recycle.Api.Controllers.Models.Category
{
    public class CategoryDetailModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
    public static class CategoryDetailModelExtentions
    {
        public static CategoryDetailModel ToDetail(this Category source)
            => new()
            {
                Id = source.Id,
                Name = source.Name,
                Description = source.Description,
            };
    }
}
