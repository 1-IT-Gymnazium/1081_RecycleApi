using Recycle.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecycleApp.Data.Entities
{
    public class Material
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public ICollection<Part> Parts { get; set; }

    }

    public static class MaterialExtentions
    {
        public static IQueryable<Material> FilterDeleted(this IQueryable<Material> query)
            => query
            ;
    }

}
