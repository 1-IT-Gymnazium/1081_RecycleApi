using Recycle.Api.Utilities;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.Products;

public class ProductUpdateModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string EAN { get; set; }
    public bool IsVerified { get; set; }
    public string? PicturePath { get; set; }
    public IEnumerable<Guid> PartIds { get; set; } = [];
}
public static class ProductUpdateModelExtensions
{
    public static ProductUpdateModel ToUpdate(this IApplicationMapper mapper, Product source)
        => new()
        {
            Name = source.Name,
            Description = source.Description,
            EAN = source.EAN,
            PicturePath = source.PicturePath,
            IsVerified = source.IsVerified,
            PartIds = source.ProductParts.Select(p => p.PartId),
        };
}

