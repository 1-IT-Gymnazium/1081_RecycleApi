using Recycle.Api.Models.Articles;
using Recycle.Data.Entities;
using System.ComponentModel.DataAnnotations;
using static Recycle.Data.Entities.TrashCan;

namespace Recycle.Api.Models.TrashCans;

public class TrashCanCreateModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "TrashCan must contain text!")]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public TrashCanType Type { get; set; }
    public string? PicturePath { get; set; }
}
public static class TrashCanCreateModelExtentions
{
    public static TrashCanCreateModel ToUpdate(this TrashCan source) => new()
    {
        Name = source.Name,
        Description = source.Description,
        Type = source.Type,
        PicturePath = source.PicturePath,
    };
}

