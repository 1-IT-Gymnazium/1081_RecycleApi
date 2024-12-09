using NodaTime;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

public class Location : ITrackable
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Regions Region { get; set; }
    public enum Regions 
    {
        A, //Praha
        S, //Středočeský kraj
        C, //Jihočeský kraj
        P, //Plzeňský kraj
        K, //Karlovarský kraj
        U, //Ústecký kraj
        L, //Liberecký kraj
        H, //Královéhradecký kraj
        E, //Pardubický kraj
        J, //Kraj Vysočina
        B, //Jihomoravský kraj
        M, //Olomoucký kraj
        Z, //Zlínský kraj
        T //Moravskoslezský kraj
    }
    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<TrashCanMaterialLocation> TrashCanMaterialLocations { get; set; }
}
public static class LocationCanExtentions
{
    public static IQueryable<Location> FilterDeleted(this IQueryable<Location> query)
        => query
        .Where(x => x.DeletedAt == null);
}
