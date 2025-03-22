using Recycle.Api.Utilities;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.Materials;

/// <summary>
/// Data returned when requesting material details, including name, description and related trash cans.
/// </summary>
public class MaterialDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; }
    public IEnumerable<Guid> TrashCanIds { get; set; } = [];

}
public static class MaterialDetailModelExtentions
{
    /// <summary>
    /// Maps a Material entity to a MaterialDetailModel for API responses.
    /// </summary>
    public static MaterialDetailModel ToDetail(this IApplicationMapper mapper, Material source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            TrashCanIds = source.TrashCanMaterials.Select(p => p.TrashCanId)

        };
}
