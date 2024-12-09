using System.ComponentModel.DataAnnotations;

namespace Recycle.Api.Models.Authorization;

public class SignInModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;

    [Required]
    public string Username { get; set; } = null!;
}
