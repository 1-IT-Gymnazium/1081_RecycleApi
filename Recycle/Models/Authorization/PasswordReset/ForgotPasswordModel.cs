namespace Recycle.Api.Models.Authorization.PasswordReset;

/// <summary>
/// Data required to initiate a password reset request.
/// </summary>
public class ForgotPasswordModel
{
    public string Email { get; set; } = string.Empty;

}
