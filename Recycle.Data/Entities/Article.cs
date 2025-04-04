using NodaTime;
using Recycle.Data.Entities.Identity;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

/// <summary>
/// Represents a written article authored by a user, with content, annotation, image, and audit info.
/// </summary>
public class Article : ITrackable
{
    public Guid Id { get; set; }
    public string Heading { get; set; } = null!;
    public string Text { get; set; } = null!;
    public string Annotation { get; set; } = null!;
    public string? PicturePath { get; set; }
    public ApplicationUser Author { get; set; } = null!;
    public Guid AuthorId { get; set; }

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}

/// <summary>
/// Filters out deleted articles (soft delete).
/// </summary>
public static class ArticleExtensions
{
    public static IQueryable<Article> FilterDeleted(this IQueryable<Article> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;
}

/// <summary>
/// Static metadata constants for article-related database configuration.
/// </summary>
public static class Metadata
{
    public const int ContentLength = DatabaseConstants.ContentLength;
    public const int TrackableByLength = DatabaseConstants.TrackableByLength;
}

