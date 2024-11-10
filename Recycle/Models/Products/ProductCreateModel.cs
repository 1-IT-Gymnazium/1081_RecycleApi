using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Products;

public class ProductCreateModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Product must contain tex!")]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string EAN { get; set; }
    public string? Description { get; set; }
    public string? PicturePath { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedBy { get; set; }
}
