using Recycle.Api.Utilities;
using Recycle.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Materials;

/// <summary>
/// Data required to create a new material,
/// including name, description, and associated trash can IDs.
/// </summary>
public class MaterialCreateModel
{
    [Required(ErrorMessage = "{0} is required.", AllowEmptyStrings = false)]
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public IEnumerable<Guid> TrashCanIds { get; set; } = [];

}

public static class MaterialCreateModelExtentions
{
    /// <summary>
    /// Provides mapping extensions for converting material entities into creation/update models.
    /// </summary>
    public static MaterialCreateModel ToUpdate(this IApplicationMapper mapper, Material source) => new()
    {
        Name = source.Name,
        Description = source.Description,
    };

}
