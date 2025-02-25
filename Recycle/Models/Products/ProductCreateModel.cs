using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Recycle.Api.Models.Products;

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
