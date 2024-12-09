using Microsoft.AspNetCore.Identity;
using NodaTime;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities.Identity;

public class ApplicationUser : IdentityUser<Guid>, ITrackable
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public ICollection<Article> Articles { get; set; } = [];
    public string Password { get; set; }
    public bool IsAdmin { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int DateOfBirth { get; set; }
    public string Email { get; set; }
    public int PhoneNumber { get; set; }
    public ICollection<UserRole> Roles { get; set; }

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; }
    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
