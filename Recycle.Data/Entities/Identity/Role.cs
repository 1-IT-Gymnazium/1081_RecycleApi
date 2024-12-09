using Microsoft.AspNetCore.Identity;
using NodaTime;
using Recycle.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities.Identity;
public class Role : IdentityRole<Guid> ,ITrackable
{
    public Guid Id { get; set; }
    public string RoleName { get; set; }
    public bool IsVerified { get; set; }
    public ICollection<UserRole> UserRoles { get; set; }

    public Instant CreatedAt { get; set; }
    public string CreatedBy { get; set; } = null!;

    public Instant ModifiedAt { get; set; }
    public string ModifiedBy { get; set; } = null!;

    public Instant? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }
}
