using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

public class TrashCanMaterialLocation
{
    public Guid Id { get; set; }
    public Guid MaterialId { get; set; }
    public Material Material { get; set; }

    public Guid TrashCanId { get; set; }
    public TrashCan TrashCan { get; set; }

    public Guid LocationId { get; set; }
    public Location Location { get; set; }
}
