using Recycle.Api.Utilities;
using Recycle.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Materials;

public class MaterialCreateModel
{
    [Required(ErrorMessage = "{0} is required.", AllowEmptyStrings = false)]
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
public static class MaterialCreateModelExtentions
{
    public static MaterialCreateModel ToUpdate(this IApplicationMapper mapper, Material source) => new()
    {
        Name = source.Name,
        Description = source.Description,
    };

}
