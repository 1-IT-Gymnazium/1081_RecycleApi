using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Authorization;

/// <summary>
/// Login credentials submitted by a user during authentication.
/// </summary>
public class LogInModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}
