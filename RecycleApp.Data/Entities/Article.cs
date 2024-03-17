using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecycleApp.Data.Entities
{
    public class Article
    {
        public Guid Id { get; set; }
        public string Heading { get; set; }
        public string Text { get; set; }
        public string AuthorsName { get; set; }
        public int DateOfCreation { get; set; }
        public string Annotation { get; set; }

    }
}
