using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Products;
using Recycle.Api.Models.Users;
using Recycle.Api.Services;
using Recycle.Data;
using Recycle.Data.Entities;
using Recycle.Data.Entities.Identity;

namespace Recycle.Api.Controllers
{
    [ApiController]
    [Route("api/v1/User")]
    public class UserController : ControllerBase
    {
        private readonly EmailSenderService _emailService;
        private readonly IClock _clock;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _dbContext;

        public UserController(
            EmailSenderService emailSenderService,
            IClock clock,
            UserManager<ApplicationUser> userManager,
            AppDbContext dbContext)
        {
            _emailService = emailSenderService;
            _clock = clock;
            _userManager = userManager;
            _dbContext = dbContext;
        }
        [Authorize]
        [HttpGet("api/v1/User/{id:guid}")]
        public async Task<ActionResult<UserDetailModel>> GetUserById(
        [FromRoute] Guid id
        )
        {
            var dbEntity = await _dbContext
                .Set<ApplicationUser>()
                .FilterDeleted()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (dbEntity == null)
            {
                return NotFound();
            };
            var user = new UserDetailModel
            {
                UserName = dbEntity.Email,
                FirstName = dbEntity.FirstName,
                LastName = dbEntity.LastName,
                Email = dbEntity.Email,
                DateOfBirth = dbEntity.DateOfBirth,
                ProfilePictureUrl = dbEntity.ProfilePictureUrl,
            };
            return Ok(user);
        }
        [Authorize]
        [HttpGet("api/v1/User/current")]
        public async Task<ActionResult<UserDetailModel>> GetCurrentUser()
        {
            // Get User ID from JWT token
            var userIdString = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(); // User is not logged in
            }

            // Convert string to Guid
            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return BadRequest("Invalid user ID format.");
            }

            // Fetch user from the database
            var dbEntity = await _dbContext
                .Set<ApplicationUser>()
                .FilterDeleted()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (dbEntity == null)
            {
                return NotFound();
            }

            // Return user details
            var user = new UserDetailModel
            {
                UserName = dbEntity.Email,
                FirstName = dbEntity.FirstName,
                LastName = dbEntity.LastName,
                Email = dbEntity.Email,
                DateOfBirth = dbEntity.DateOfBirth,
                ProfilePictureUrl = dbEntity.ProfilePictureUrl,
                IsAdmin = dbEntity.IsAdmin
            };

            return Ok(user);
        }

        [Authorize]
        [HttpPatch("UserChanges/UpdateUsername")]
        public async Task<ActionResult> UpdateUsername(string userId, [FromBody] string newUsername)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "USER_NOT_FOUND");
                return ValidationProblem(ModelState);
            }

            user.UserName = newUsername;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "UPDATE_FAILED");
                return ValidationProblem(ModelState);
            }

            return NoContent();
        }

        [Authorize]
        [HttpPatch("UserChanges/UpdateEmail")]
        public async Task<ActionResult> UpdateEmail(string userId, [FromBody] string newEmail)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "USER_NOT_FOUND");
                return ValidationProblem(ModelState);
            }

            user.Email = newEmail;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "UPDATE_FAILED");
                return ValidationProblem(ModelState);
            }

            return NoContent();
        }

        [Authorize]
        [HttpPatch("UserChanges/UpdateProfilePicture")]
        public async Task<ActionResult> UpdateProfilePicture(string userId, [FromBody] string profilePictureUrl)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "USER_NOT_FOUND");
                return ValidationProblem(ModelState);
            }

            user.ProfilePictureUrl = profilePictureUrl;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "UPDATE_FAILED");
                return ValidationProblem(ModelState);
            }

            return NoContent();
        }

        [Authorize]
        [HttpPatch("UserChanges/UpdatePassword")]
        public async Task<ActionResult> UpdatePassword(string userId, [FromBody] UpdateUserPasswordModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "USER_NOT_FOUND");
                return ValidationProblem(ModelState);
            }

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

            return NoContent();
        }
    }
}
