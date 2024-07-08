using Recycle.Api.Controllers.Models.Articles;
using RecycleApp.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Controllers.Models.Category
{
    public class CategoryCreateModel
    {
        [Required(ErrorMessage = "{0} is required.", AllowEmptyStrings = false)]
        public string Name { get; set; } = null!;

        public string? Description { get; set; }
    }
    public static class CategoryCreateModelExtentions  
    {
        public static CategoryCreateModel ToUpdate(this Category source) => new()
        {
            Name = source.Name,
            Description = source.Description,
        };

    }
}
