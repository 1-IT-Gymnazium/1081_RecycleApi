using NodaTime;

namespace Recycle.Data.Interfaces;

/// <summary>
/// Provides helper methods to manage ITrackable metadata with optional system defaults.
/// </summary>
public interface ITrackable
{
    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } 
    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

public static class ITrackableExtensions
{
    private const string SYSTEM = "System";

    /// <summary>
    /// Sets creation and modification metadata with "System" as the author.
    /// </summary>
    public static T SetCreateBySystem<T>(this T trackable, Instant now)
        where T : class, ITrackable
        => trackable.SetCreateBy(SYSTEM, now);

    /// <summary>
    /// Sets modification metadata with "System" as the author.
    /// </summary>
    public static T SetModifyBySystem<T>(this T trackable, Instant now)
        where T : class, ITrackable
        => trackable.SetModifyBy(SYSTEM, now);

    /// <summary>
    /// Sets deletion metadata with "System" as the author.
    /// </summary>
    public static T SetDeleteBySystem<T>(this T trackable, Instant now)
        where T : class, ITrackable
        => trackable.SetDeleteBy(SYSTEM, now);

    /// <summary>
    /// Sets creation and modification metadata using a custom author.
    /// </summary>
    public static T SetCreateBy<T>(this T trackable, string author, Instant now)
        where T : class, ITrackable
    {
        trackable.CreatedAt = now;
        trackable.CreatedBy = author;

        return trackable.SetModifyBy(author, now);
    }

    /// <summary>
    /// Sets modification metadata using a custom author.
    /// </summary>
    public static T SetModifyBy<T>(this T trackable, string author, Instant now)
        where T : class, ITrackable
    {
        trackable.ModifiedAt = now;
        trackable.ModifiedBy = author;
        return trackable;
    }

    /// <summary>
    /// Sets deletion metadata using a custom author.
    /// </summary>
    public static T SetDeleteBy<T>(this T trackable, string author, Instant now)
        where T : class, ITrackable
    {
        trackable.DeletedAt = now;
        trackable.DeletedBy = author;

        return trackable;
    }
}

