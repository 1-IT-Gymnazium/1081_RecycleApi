using Org.BouncyCastle.Crypto;
using Recycle.Api.Models.Materials;
using Recycle.Api.Utilities;
using Recycle.Data.Entities;
using System.Xml.Linq;

namespace Recycle.Api.Models.Parts;

public class PartDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null;
    public string Type { get; set; }
    public string Description { get; set; }
    public bool IsVerified { get; set; }
    public string? PicturePath { get; set; }
    public List<IdNameModel> TrashCans { get; set; } = [];
    public MaterialSimple Material { get; set; } = new();
}
public class PartFilter
{
    public Guid? ProductId { get; set; }
}

public static class PartDetailModelExtensions
{
    public static IQueryable<Part> ApplyFilter(this IQueryable<Part> query, PartFilter? filter)
    {
        if (filter != null)
        {
            if (filter.ProductId != null)
            {
                query = query.Where(x => x.ProductParts.Any(y => y.ProductId == filter.ProductId));
            }
        }

        return query;
    }

    public static PartDetailModel ToDetail(this IApplicationMapper mapper, Part source)
    {
        var result = new PartDetailModel()
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            IsVerified = source.IsVerified,
            PicturePath = source.PicturePath,
            TrashCans = source.PartMaterial.TrashCanMaterials
                .Select(tm => new IdNameModel
                {
                    Id = tm.TrashCan.Id,
                    Name = tm.TrashCan.Name
                })
                .DistinctBy(tc => tc.Id)
                .ToList()
        };

        return result;
    }
}
