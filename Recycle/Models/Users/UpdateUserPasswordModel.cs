namespace Recycle.Api.Models.Users;

public class UpdateUserPasswordModel
{
    public string OldPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
