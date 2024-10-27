using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

public class Material
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public Guid PartId { get; set; }
    public Part Part { get; set; }

    public ICollection<TrashCanMaterialLocation> TrashCanMaterialLocations { get; set; }
}

public static class MaterialExtentions
{
    public static IQueryable<Material> FilterDeleted(this IQueryable<Material> query)
        => query
        ;
}
