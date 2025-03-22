namespace Recycle.Api.Models;

/// <summary>
/// Simple ID-name pair used for lightweight references across entities (e.g. dropdowns, lookups).
/// </summary>
public class IdNameModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
