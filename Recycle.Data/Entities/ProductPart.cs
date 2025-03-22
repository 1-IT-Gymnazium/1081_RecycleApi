using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

/// <summary>
/// Represents the many-to-many relationship between products and parts.
/// </summary>
public class ProductPart
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }

    public Guid PartId { get; set; }
    public Part Part { get; set; }
}
