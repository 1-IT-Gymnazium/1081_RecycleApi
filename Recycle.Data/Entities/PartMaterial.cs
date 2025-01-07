using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;
public class PartMaterial
{
    public Guid Id { get; set; }
    public Guid PartId { get; set; }
    public Part Part { get; set; }
    public Guid MaterialId { get; set; }
    public Material Material { get; set; }
}
