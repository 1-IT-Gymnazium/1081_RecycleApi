using Recycle.Api.Models.Articles;
using Recycle.Api.Utilities;
using Recycle.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Products;

public class ProductDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; }
    public string EAN { get; set; }
    public bool IsVerified { get; set; }
    public string? PicturePath { get; set; }
    public IEnumerable<IdNameModel> Parts { get; set; } = [];
}
public static class ProductDetailModelExtensions
{
    public static ProductDetailModel ToDetail(this IApplicationMapper mapper, Product source)
    {
        string baseUrl = "http://localhost:5100";

        return new ProductDetailModel
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            EAN = source.EAN,
            PicturePath = string.IsNullOrEmpty(source.PicturePath) ? null : $"{baseUrl}{source.PicturePath}",
            IsVerified = source.IsVerified,
            Parts = source.ProductParts.Select(p => new IdNameModel
            {
                Id = p.Part.Id,
                Name = p.Part.Name,
            })
        };
    }
}
