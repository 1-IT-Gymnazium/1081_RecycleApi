using RecycleApp.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities
{
    public class Part
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public enum Type 
        { 
            Wrapping, 
            Part
        }

        public ICollection<Product> Products { get; set; }

        public Guid MaterialId { get; set; }
        public Material Material { get; set; }

    }
}
