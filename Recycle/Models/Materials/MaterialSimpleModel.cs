namespace Recycle.Api.Models.Materials;

public class MaterialSimple
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public IEnumerable<Guid> TrashCanIds { get; set; } = [];
}
