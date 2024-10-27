using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Authorization;

public class LogInModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
}
