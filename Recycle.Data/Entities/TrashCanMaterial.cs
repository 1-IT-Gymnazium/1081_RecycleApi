using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

/// <summary>
/// Represents the many-to-many relationship between trash cans and materials.
/// </summary>
public class TrashCanMaterial
{
    public Guid Id { get; set; }
    public Guid MaterialId { get; set; }
    public Material Material { get; set; }

    public Guid TrashCanId { get; set; }
    public TrashCan TrashCan { get; set; }
}
