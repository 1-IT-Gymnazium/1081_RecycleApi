using Org.BouncyCastle.Crypto;
using Recycle.Api.Models.Materials;
using Recycle.Api.Utilities;
using Recycle.Data.Entities;
using System.Xml.Linq;

namespace Recycle.Api.Models.Parts;

public class PartProductDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? PicturePath { get; set; }
    public IEnumerable<IdNameModel> TrashCans { get; set; } = [];
    public string MaterialName { get; set; } = null!;
}

public static class PartProductDetailModelExtensions
{
    
}
