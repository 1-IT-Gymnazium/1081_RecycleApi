namespace Recycle.Api.Models.Users;

public class UserDetailModel
{
    public string UserName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string DateOfBirth { get; set; } = null!;
    public string? ProfilePictureUrl {  get; set; }
    public bool IsAdmin { get; set; }
}
