using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities
{
    public class Location
    {
        public Guid Id { get; set; }
        public enum Region 
        {
            A, //Praha
            S, //Středočeský kraj
            C, //Jihočeský kraj
            P, //Plzeňský kraj
            K, //Karlovarský kraj
            U, //Ústecký kraj
            L, //Liberecký kraj
            H, //Královéhradecký kraj
            E, //Pardubický kraj
            J, //Kraj Vysočina
            B, //Jihomoravský kraj
            M, //Olomoucký kraj
            Z, //Zlínský kraj
            T //Moravskoslezský kraj
        }

    }
}
