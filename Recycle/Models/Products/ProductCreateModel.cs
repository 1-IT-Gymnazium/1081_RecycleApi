namespace Recycle.Api.Models.Products;

public class ProductCreateModel
{
    public string Name { get; set; }
    public int EAN { get; set; }
    public string? Description { get; set; }
    public string? PicturePath { get; set; }
    public string CreatedBy { get; set; }
    public string ModifiedBy { get; set; }
}
