using Recycle.Api.Utilities;
using Recycle.Data.Entities;
using static Recycle.Data.Entities.TrashCan;

namespace Recycle.Api.Models.TrashCans;

/// <summary>
/// Data returned when requesting trash can details, including type, description, and optional image.
/// </summary>
public class TrashCanDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Description { get; set; }
    public string? PicturePath { get; set; }
}

/// <summary>
/// Maps a TrashCan entity to a detail model for API responses.
/// </summary>
public static class TrashCanDetailModelExtensions
{
    public static TrashCanDetailModel ToDetail(this IApplicationMapper mapper, TrashCan source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
            Type = source.Type,
            Description = source.Description,
            PicturePath = string.IsNullOrEmpty(source.PicturePath) ? null : $"{mapper.EnviromentSettings.BackendHostUrl}{source.PicturePath}",
        };
}
