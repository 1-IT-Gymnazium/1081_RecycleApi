using NodaTime;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

/// <summary>
/// Represents a product part, including its material, verification status, and audit metadata.
/// </summary>
public class Part : ITrackable
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsVerified { get; set; }  
    public string? PicturePath { get; set; }

    public string Type { get; set; }

    public ICollection<ProductPart> ProductParts { get; set; } = [];
    public Material PartMaterial { get; set; } = null!;
    public Guid PartMaterialId { get; set; }
    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Filters out soft-deleted parts from a query.
/// </summary>
public static class PartExtentions
{
    public static IQueryable<Part> FilterDeleted(this IQueryable<Part> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;
}

