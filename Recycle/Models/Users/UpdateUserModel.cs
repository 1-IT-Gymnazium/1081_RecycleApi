namespace Recycle.Api.Models.Users;

public class UpdateUserModel
{
    public string FirstName { get; set; } = null!;  
    public string LastName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;

    public string ProfilePictureUrl { get; set; } = null!;
    public string OldPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
}
