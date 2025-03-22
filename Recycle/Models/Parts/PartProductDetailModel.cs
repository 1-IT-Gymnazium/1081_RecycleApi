using Org.BouncyCastle.Crypto;
using Recycle.Api.Models.Materials;
using Recycle.Api.Utilities;
using Recycle.Data.Entities;
using System.Xml.Linq;

namespace Recycle.Api.Models.Parts;

/// <summary>
/// Data used when displaying a part inside a product, including name, material, image, and related trash cans.
/// </summary>
public class PartProductDetailModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? PicturePath { get; set; }
    public IEnumerable<IdNameModel> TrashCans { get; set; } = [];
    public string MaterialName { get; set; } = null!;
}

