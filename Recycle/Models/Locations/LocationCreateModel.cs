using Recycle.Data.Entities;
using static Recycle.Data.Entities.TrashCan;
using System.ComponentModel.DataAnnotations;
using static Recycle.Data.Entities.Location;

namespace Recycle.Api.Models.Locations;

public class LocationCreateModel
{
    // [Required(AllowEmptyStrings = false, ErrorMessage = "TrashCan must contain text!")]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Regions Region { get; set; }
}
public static class LocationCreateModelExtentions
{
    public static LocationCreateModel ToUpdate(this Location source) => new()
    {
        Name = source.Name,
        Region = source.Region,
    };
}
