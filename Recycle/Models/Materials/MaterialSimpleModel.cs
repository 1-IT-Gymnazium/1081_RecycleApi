namespace Recycle.Api.Models.Materials;

/// <summary>
/// Minimal material data used for lightweight listings or lookups.
/// </summary>
public class MaterialSimple
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<Guid> TrashCanIds { get; set; } = [];
}
