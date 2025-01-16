using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using Recycle.Api.Models.Users;
using Recycle.Api.Services;
using Recycle.Data.Entities.Identity;

namespace Recycle.Api.Controllers
{
    public class UserChangesController : ControllerBase
    {
        private readonly EmailSenderService _emailService;
        private readonly IClock _clock;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserChangesController(
            EmailSenderService emailSenderService,
            IClock clock,
            UserManager<ApplicationUser> userManager)
        {
            _emailService = emailSenderService;
            _clock = clock;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPatch("api/v1/UserChanges/UpdateUser")]
        public async Task<ActionResult> UpdateUser(string userId, [FromBody] UpdateUserModel model)
        {
            // Find the user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "USER_NOT_FOUND");
                return ValidationProblem(ModelState);
            }

            // Update username if provided
            if (!string.IsNullOrEmpty(model.UserName))
            {
                user.UserName = model.UserName;
            }

            // Update email if provided
            if (!string.IsNullOrEmpty(model.Email))
            {
                user.Email = model.Email;
            }

            // Update profile picture URL if provided
            if (!string.IsNullOrEmpty(model.ProfilePictureUrl))
            {
                user.ProfilePictureUrl = model.ProfilePictureUrl;
            }

            // Handle password change
            if (!string.IsNullOrEmpty(model.OldPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var passwordCheck = await _userManager.CheckPasswordAsync(user, model.OldPassword);
                if (!passwordCheck)
                {
                    ModelState.AddModelError(string.Empty, "INVALID_OLD_PASSWORD");
                    return ValidationProblem(ModelState);
                }

                var passwordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    ModelState.AddModelError(string.Empty, "PASSWORD_CHANGE_FAILED");
                    return ValidationProblem(ModelState);
                }
            }

            // Save changes for other updates
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "UPDATE_FAILED");
                return ValidationProblem(ModelState);
            }

            return NoContent();
        }
    }
}
