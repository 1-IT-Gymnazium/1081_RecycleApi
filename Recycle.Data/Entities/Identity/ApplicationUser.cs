using Microsoft.AspNetCore.Identity;
using NodaTime;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>, ITrackable
{
    public ICollection<Article> Articles { get; set; } = [];
    public string DisplayName { get; set; } = null!;
    public bool IsAdmin { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string DateOfBirth { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;
    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;
    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
public static class UserExtentions
{
    public static IQueryable<ApplicationUser> FilterDeleted(this IQueryable<ApplicationUser> query)
        => query
        .Where(x => x.DeletedAt == null)
        ;
}
