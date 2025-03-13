using Recycle.Api.Models.Materials;
using Recycle.Api.Utilities;
using Recycle.Data.Entities;

namespace Recycle.Api.Models.Parts;

public class PartUpdateModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null;
    public string Type { get; set; }
    public string Description { get; set; }
    public bool IsVerified { get; set; }
    public string? PicturePath { get; set; }
    public List<IdNameModel> TrashCans { get; set; } = new List<IdNameModel>();
    public Guid MaterialId { get; set; }
}
public static class PartUpdateModelExtensions
    {
        public static PartUpdateModel ToUpdate(this IApplicationMapper mapper, Part source)
        {
            var result = new PartUpdateModel()
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

