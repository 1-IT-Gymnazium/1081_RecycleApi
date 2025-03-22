namespace Recycle.Api.Models.Authorization;

/// <summary>
/// Data required to register a new user account.
/// </summary>
public class RegisterModel
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string DateOfBirth { get; set; } = null!;
    }
