using Recycle.Api.Utilities;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.Products;

public class ProductViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string EAN { get; set; }
    public bool IsVerified { get; set; }
}
public static class ProductViewlModelExtensions
{
    public static ProductViewModel ToView(this IApplicationMapper mapper, Product source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
            EAN = source.EAN,
            IsVerified = source.IsVerified,
        };
}

