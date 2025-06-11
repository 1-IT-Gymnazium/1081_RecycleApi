using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recycle.Api.Models.Products;

/// <summary>
/// Data required to create a new product, including name, EAN, parts, and optional image or description.
/// </summary>
public class ProductCreateModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Product must contain text!")]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string EAN { get; set; }
    public string? Description { get; set; }
    public string? PicturePath { get; set; }
    public bool? IsVerified { get; set; }
    public IEnumerable<Guid> PartIds { get; set; } = [];
}
