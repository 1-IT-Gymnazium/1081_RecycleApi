using Recycle.Api.Models.Parts;
using Recycle.Api.Utilities;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.Products;

/// <summary>
/// Data used to update an existing product, including name, EAN, verification state, and associated parts.
/// </summary>
public class ProductUpdateModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string EAN { get; set; }
    public bool IsVerified { get; set; }
    public string? PicturePath { get; set; }
    public IEnumerable<PartProductUpdateModel>? Parts { get; set; } = [];
}

/// <summary>
/// Maps a Product entity to an update model with editable fields.
/// </summary>
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
            Parts = source.ProductParts.Select(p => new PartProductUpdateModel
            {
                Id = p.Part.Id,
                Name = p.Part.Name,
            })
        };
}

