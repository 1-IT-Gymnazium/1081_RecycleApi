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
    /// <summary>
    /// Controller for managing user-related actions such as updating profile info,
    /// retrieving user details, and handling profile picture uploads.
    /// </summary>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class with required services.
        /// </summary>
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

        /// <summary>
        /// Retrieves basic account info of the currently authenticated user.
        /// If the user is not authenticated, returns null fields and IsAdmin = false.
        /// </summary>
        /// <returns>
        /// Returns 200 (OK) with user detail model or default empty data if not logged in.
        /// </returns>
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
                ProfilePictureUrl = (string.IsNullOrEmpty(user.ProfilePictureUrl))? string.Empty : $"{_mapper.EnviromentSettings.BackendHostUrl}{user.ProfilePictureUrl}",
                IsAdmin = user.IsAdmin,
            };
        }

        private async Task<ApplicationUser?> GetAuthenticatedUser()
        {
            var userId = User?.FindFirst("sub")?.Value; 

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

        /// <summary>
        /// Updates the username of the currently authenticated user.
        /// </summary>
        /// <param name="newUsername">The new username to be assigned.</param>
        /// <returns>
        /// Returns 204 (NoContent) on success, 400 (BadRequest) if taken or failed.
        /// </returns>
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

        /// <summary>
        /// Updates the email of the currently authenticated user.
        /// </summary>
        /// <param name="newEmail">The new email to be assigned.</param>
        /// <returns>
        /// Returns 204 (NoContent) on success, 400 (BadRequest) if taken or failed.
        /// </returns>
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
        /// <summary>
        /// Updates the password of the currently authenticated user after verifying the old password.
        /// </summary>
        /// <param name="model">Model containing old and new passwords.</param>
        /// <returns>
        /// Returns 204 (NoContent) on success, 400 (BadRequest) if old password is incorrect or update fails.
        /// </returns>
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
        /// <summary>
        /// Updates the profile picture of the currently authenticated user.
        /// </summary>
        /// <param name="profilePicture">Uploaded image file as profile picture.</param>
        /// <returns>
        /// Returns 200 (OK) with new image path or 400 (BadRequest) if upload fails.
        /// </returns>
        [HttpPatch("api/v1/User/UpdateProfilePicture")]
        public async Task<IActionResult> UpdateProfilePicture([FromForm] IFormFile profilePicture) 
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

            // ✅ Save the new profile picture
            var newImagePath = await _imageService.SaveImageAsync(profilePicture, "ProfilePictures");

            // ✅ Delete old profile picture if exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                await _imageService.DeleteImageAsync(user.ProfilePictureUrl);
            }

            // ✅ Update user profile picture path
            user.ProfilePictureUrl = newImagePath;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "✅ Profile picture updated successfully.", imagePath = newImagePath });
        }
    }
}
