using Org.BouncyCastle.Crypto;
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
    public List<IdNameModel> TrashCans { get; set; } = new List<IdNameModel>();
    public IEnumerable<Guid> MaterialIds { get; set; } = [];

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
            TrashCans = source.PartMaterials
                .SelectMany(pm => pm.Material.TrashCanMaterials)
                .Select(tm => new IdNameModel
                {
                    Id = tm.TrashCan.Id,
                    Name = tm.TrashCan.Name
                })
                .DistinctBy(tc => tc.Id)  // Ensures no duplicate TrashCans by Id
                .ToList()               // Converts to a List
        };

        return result;
        //    var result = new PartDetailModel()
        //    {
        //        Id = source.Id,
        //        Name = source.Name,
        //        Description = source.Description,
        //        IsVerified = source.IsVerified,
        //        PicturePath = source.PicturePath,
        //        // misto matid potrebuju vytanhnout popelnice do kolekce
        //        MaterialIds = source.PartMaterials.Select(p => p.MaterialId)

        //    };
        //    return result;
        //}
    }
}
