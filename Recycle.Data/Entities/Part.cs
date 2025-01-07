using NodaTime;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

public class Part : ITrackable
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsVerified { get; set; }  
    public string PicturePath { get; set; }

    public PartType Type { get; set; }

    public ICollection<ProductPart> ProductParts { get; set; }
    public ICollection<PartMaterial> PartMaterials { get; set; }
    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public enum PartType
{
    Wrapping = 1,
    Part = 2,
}
public static class PartExtentions
{
    public static IQueryable<Part> FilterDeleted(this IQueryable<Part> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;
}

