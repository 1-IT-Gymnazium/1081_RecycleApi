using Recycle.Api.Models.Articles;
using Recycle.Data.Entities;
using System.ComponentModel.DataAnnotations;
using static Recycle.Data.Entities.TrashCan;

namespace Recycle.Api.Models.TrashCans;

/// <summary>
/// Data required to create a new trash can, including name, type, image, and optional description.
/// </summary>
public class TrashCanCreateModel
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "TrashCan must contain text!")]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string Type { get; set; } = null!;
    public string? PicturePath { get; set; }
}

/// <summary>
/// Maps a TrashCan entity to a create/update model with editable fields.
/// </summary>
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

