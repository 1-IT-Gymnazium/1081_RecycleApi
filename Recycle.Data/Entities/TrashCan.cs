using NodaTime;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

/// Represents a physical trash can used for collecting recyclable materials. 
/// Includes type, description, image, and links to associated materials.
/// </summary>
public class TrashCan : ITrackable
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public string Description { get; set; }
    public string? PicturePath { get; set; }

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<TrashCanMaterial> TrashCanMaterials { get; set; }
}

/// <summary>
/// Filters out soft-deleted products from a query.
/// </summary>
public static class TrashCanExtentions
{
    public static IQueryable<TrashCan> FilterDeleted(this IQueryable<TrashCan> query)
        => query
        .Where(x => x.DeletedAt == null);
}

