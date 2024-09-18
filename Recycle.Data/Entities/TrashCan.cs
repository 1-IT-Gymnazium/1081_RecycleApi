using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities
{
    public class TrashCan
    {

        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

    }
}
