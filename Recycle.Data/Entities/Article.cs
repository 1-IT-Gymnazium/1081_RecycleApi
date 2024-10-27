﻿using NodaTime;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities;

public class Article : ITrackable
{
    public Guid Id { get; set; }
    public string Heading { get; set; } = null!;
    public string Text { get; set; } = null!;
    public string AuthorsName { get; set; } = null!;
    public string Annotation { get; set; } = null!;

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
public static class ArticleExtensions
{
    public static IQueryable<Article> FilterDeleted(this IQueryable<Article> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;
}
