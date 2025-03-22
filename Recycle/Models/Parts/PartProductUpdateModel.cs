using Org.BouncyCastle.Crypto;
using Recycle.Api.Models.Materials;
using Recycle.Api.Utilities;
using Recycle.Data.Entities;
using System.Xml.Linq;

namespace Recycle.Api.Models.Parts;

/// <summary>
/// Data used to update basic part information inside a product context.
/// </summary>
public class PartProductUpdateModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}

