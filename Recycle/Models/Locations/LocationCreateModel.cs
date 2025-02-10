using Recycle.Data.Entities;
using static Recycle.Data.Entities.TrashCan;
using System.ComponentModel.DataAnnotations;
using static Recycle.Data.Entities.Location;

namespace Recycle.Api.Models.Locations;

public class LocationCreateModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Location must contain text!")]
    public string Name { get; set; }
}
public static class LocationCreateModelExtentions
{
    public static LocationCreateModel ToCreate(this Location source) => new()
    {
        Name = source.Name,
    };
}
