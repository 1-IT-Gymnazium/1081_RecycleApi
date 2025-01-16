namespace Recycle.Api.Models.Users;

public class UpdateUserModel
{
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string ProfilePictureUrl { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}
