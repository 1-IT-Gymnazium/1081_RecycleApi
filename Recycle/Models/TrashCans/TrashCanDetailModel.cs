using Recycle.Api.Utilities;
using Recycle.Data.Entities;
using static Recycle.Data.Entities.TrashCan;

namespace Recycle.Api.Models.TrashCans;

public class TrashCanDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Description { get; set; }
    public string? PicturePath { get; set; }
}
public static class TrashCanDetailModelExtensions
{
    public static TrashCanDetailModel ToDetail(this IApplicationMapper mapper, TrashCan source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
            Type = source.Type,
            Description = source.Description,
            PicturePath = source.PicturePath,
        };
}
