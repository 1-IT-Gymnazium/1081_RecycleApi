using Recycle.Api.Utilities;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.TrashCans;

public class TrashCanViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string PicturePath { get; set; }
}
public static class TrashCanViewlModelExtensions
{
    public static TrashCanViewModel ToView(this IApplicationMapper mapper, TrashCan source)
        => new()
        {
            Id = source.Id,
            Name = source.Name,
            PicturePath = source.PicturePath,
        };
}
