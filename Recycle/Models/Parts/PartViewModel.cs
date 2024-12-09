using Recycle.Api.Models.Products;
using Recycle.Api.Utilities;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.Parts;

public class PartViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsVerified { get; set; }
}
public static class PartViewlModelExtensions
{
    public static PartViewModel ToView(this IApplicationMapper mapper, Part source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
            IsVerified = source.IsVerified,
        };

}
