using Recycle.Data.Entities.Identity;

namespace Recycle.Api.Services;

public interface IAuthEmailService
{
    Task SendRegistrationConfirmationEmailAsync(ApplicationUser user, string token);
    Task SendPasswordResetEmailAsync(string email, string token);
}

