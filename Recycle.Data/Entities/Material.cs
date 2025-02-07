using NodaTime;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

public class Material : ITrackable
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public ICollection<PartMaterial> PartMaterials { get; set; }
    public ICollection<TrashCanMaterial> TrashCanMaterials { get; set; }
}

public static class MaterialExtentions
{
    public static IQueryable<Material> FilterDeleted(this IQueryable<Material> query)
        => query
        ;
}
