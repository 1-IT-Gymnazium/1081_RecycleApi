using NodaTime;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Recycle.Data.Entities.Identity;

/// <summary>
/// Represents a refresh token issued to a user, including expiration and revocation metadata.
/// </summary>
[Table(nameof(RefreshToken))]
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Token { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant ExpiresAt { get; set; }
    public Instant? RevokedAt { get; set; }
    public string? RequestInfo { get; set; }
}
