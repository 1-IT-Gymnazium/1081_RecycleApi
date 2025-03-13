using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Users;
using Recycle.Api.Services;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities.Identity;
using System.Security.Claims;

namespace Recycle.Api.Controllers
{
    [ApiController]
    [Authorize] // Ensures user is authenticated for protected routes
    [Route("api/v1/User")]
    public class UserController : ControllerBase
    {
        private readonly EmailSenderService _emailService;
        private readonly IClock _clock;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _dbContext;
        private readonly IImageService _imageService;
        private readonly IApplicationMapper _mapper;

        public UserController(
            EmailSenderService emailSenderService,
            IClock clock,
            UserManager<ApplicationUser> userManager,
            AppDbContext dbContext,
            IImageService imageService,
            IApplicationMapper mapper)
        {
            _emailService = emailSenderService;
            _clock = clock;
            _userManager = userManager;
            _dbContext = dbContext;
            _imageService = imageService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet("UserInfo")]
        public async Task<ActionResult<UserDetailModel>> GetAccountInfo()
        {
            if (!User.Identities.Any(x => x.IsAuthenticated))
            {
                return new UserDetailModel
                {
                    UserName = null,
                    FirstName = null,
                    LastName = null,
                    Email = null,
                    DateOfBirth = null,
                    ProfilePictureUrl = null,
                    IsAdmin = false,
                };
            }

            var id = User.GetUserId();
            var user = await _userManager.Users
                .Where(x => x.Id == id)
                .AsNoTracking()
                .SingleAsync();

            return new UserDetailModel
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DateOfBirth = user.DateOfBirth,
                ProfilePictureUrl = $"{_mapper.EnviromentSettings.BackendHostUrl}{user.ProfilePictureUrl}",
                IsAdmin = user.IsAdmin,
            };
        }

        private async Task<ApplicationUser?> GetAuthenticatedUser()
        {
            var userId = User?.FindFirst("sub")?.Value; // Try finding "sub" claim first

            if (string.IsNullOrEmpty(userId))
            {
                userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value; // Fallback to NameIdentifier
            }

            if (string.IsNullOrEmpty(userId))
            {
                return null; // If still empty, return null
            }

            return await _userManager.FindByIdAsync(userId);
        }
        [HttpPatch("UpdateUsername")]
        public async Task<IActionResult> UpdateUsername([FromBody] string newUsername)
        {
            var user = await GetAuthenticatedUser();
            if (user == null)
            {
                return NotFound(new { error = "USER_NOT_FOUND", message = "User not found." });
            }

            var existingUser = await _userManager.FindByNameAsync(newUsername);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return BadRequest(new { error = "DUPLICATE_USERNAME", message = $"The username '{newUsername}' is already taken." });
            }

            user.UserName = newUsername;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? NoContent() : BadRequest(new { error = "UPDATE_FAILED", message = "Failed to update username." });
        }
        [HttpPatch("UpdateEmail")]
        public async Task<IActionResult> UpdateEmail([FromBody] string newEmail)
        {
            var user = await GetAuthenticatedUser();
            if (user == null)
            {
                return NotFound(new { error = "USER_NOT_FOUND", message = "User not found." });
            }

            var existingUser = await _userManager.FindByEmailAsync(newEmail);
            if (existingUser != null && existingUser.Id != user.Id)
            {
                return BadRequest(new { error = "DUPLICATE_EMAIL", message = $"The email '{newEmail}' is already in use." });
            }

            user.Email = newEmail;
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? NoContent() : BadRequest(new { error = "UPDATE_FAILED", message = "Failed to update email." });
        }

        [HttpPatch("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdateUserPasswordModel model)
        {
            var user = await GetAuthenticatedUser();
            if (user == null)
            {
                return NotFound(new { error = "USER_NOT_FOUND", message = "User not found." });
            }

            var passwordCheck = await _userManager.CheckPasswordAsync(user, model.OldPassword);
            if (!passwordCheck)
            {
                return BadRequest(new { error = "INVALID_OLD_PASSWORD", message = "The old password is incorrect." });
            }

            var passwordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            return passwordResult.Succeeded ? NoContent() : BadRequest(new { error = "PASSWORD_CHANGE_FAILED", message = "Failed to update password." });
        }

        [HttpPatch("api/v1/User/UpdateProfilePicture")]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile profilePicture)
        {
            var user = await GetAuthenticatedUser();
            if (user == null)
            {
                return NotFound(new { error = "USER_NOT_FOUND", message = "User not found." });
            }

            if (profilePicture == null || profilePicture.Length == 0)
            {
                return BadRequest(new { error = "NO_FILE_UPLOADED", message = "No profile picture uploaded." });
            }

            // Save the new profile picture
            var newImagePath = await _imageService.SaveImageAsync(profilePicture, "UserProfileImages");

            // Delete old profile picture if exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                await _imageService.DeleteImageAsync(user.ProfilePictureUrl);
            }

            // Update user profile picture path
            user.ProfilePictureUrl = newImagePath;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Profile picture updated successfully.", imagePath = newImagePath });
        }
    }
}
