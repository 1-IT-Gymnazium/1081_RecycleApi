using Recycle.Data.Entities;

namespace Recycle.Api.Models.Materials;

public class MaterialDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; }
}
public static class CategoryDetailModelExtentions
{
    public static MaterialDetailModel ToDetail(this Material source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
        };
}
