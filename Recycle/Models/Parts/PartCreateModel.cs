using Recycle.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Parts;

public class PartCreateModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "Part must contain text!")]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? PicturePath { get; set; }
    public string Type { get; set; }
    public IEnumerable<Guid> MaterialIds{ get; set; } = [];

}
