using Recycle.Api.Models.Articles;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.Products;

public class ProductDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } 
    public string EAN { get; set; }
    public bool IsVerified { get; set; }
    public string? PicturePath { get; set; }
}
public static class ProductDetailModelExtensions
{
    public static ProductDetailModel ToDetail(this Product source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            EAN = source.EAN,
            PicturePath = source.PicturePath,
            IsVerified = source.IsVerified,
        };
}
