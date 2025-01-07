using Recycle.Api.Utilities;
using Recycle.Data.Entities;
using static Recycle.Data.Entities.Location;
using static Recycle.Data.Entities.TrashCan;

namespace Recycle.Api.Models.Locations;

public class LocationDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
public static class LocationDetailModelExtensions
{
    public static LocationDetailModel ToDetail(this IApplicationMapper mapper, Location source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
        };
}
