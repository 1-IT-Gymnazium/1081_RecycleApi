using NodaTime;
using Recycle.Data.Entities.Identity;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

/// <summary>
/// Represents a product composed of multiple parts, identified by an EAN code and supporting audit metadata.
/// </summary>
public class Product : ITrackable
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Name { get; set; }

    [RegularExpression(@"^\d+$", ErrorMessage = "EAN must be numeric.")]
    public string EAN { get; set; }
    public string? Description { get; set; }
    public bool IsVerified { get; set; }
    public string? PicturePath { get; set; }

    public ICollection<ProductPart> ProductParts { get; set; } = new HashSet<ProductPart>();

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Filters out soft-deleted products from a query.
/// </summary>
public static class ProductExtentions
{
    public static IQueryable<Product> FilterDeleted(this IQueryable<Product> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;
}

