using Recycle.Api.Utilities;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.Parts;

public class PartDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null;
    public string Type { get; set; }
    public string Description { get; set; }
    public bool IsVerified { get; set; }
    public string? PicturePath { get; set; }
    public IEnumerable<Guid> MaterialIds { get; set; } = [];

}
public static class ParttDetailModelExtensions
{
    public static PartDetailModel ToDetail(this IApplicationMapper mapper, Part source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            IsVerified = source.IsVerified,
            PicturePath = source.PicturePath,
            MaterialIds = source.PartMaterials.Select(p => p.MaterialId)

        };
}
