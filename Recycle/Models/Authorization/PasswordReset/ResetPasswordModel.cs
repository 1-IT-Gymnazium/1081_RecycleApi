namespace Recycle.Api.Models.Authorization.PasswordReset;

/// <summary>
/// Data required to reset a user's password using a token.
/// </summary>
public class ResetPasswordModel
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
