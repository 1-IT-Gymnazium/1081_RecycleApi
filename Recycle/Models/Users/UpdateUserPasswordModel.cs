namespace Recycle.Api.Models.Users;

/// <summary>
/// Data used to change the user's password by providing the old and new password values.
/// </summary>
public class UpdateUserPasswordModel
{
    public string OldPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
