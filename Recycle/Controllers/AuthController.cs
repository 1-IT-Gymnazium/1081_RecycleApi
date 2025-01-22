using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using Recycle.Api.Models.Authorization;
using Recycle.Api.Services;
using Recycle.Api.Settings;
using Recycle.Api.Utilities;
using Recycle.Data.Entities.Identity;
using Recycle.Data.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Recycle.Api.Controllers;
[ApiController]
public class AuthController : ControllerBase
{
    private readonly EmailSenderService _emailService;
    private readonly IClock _clock;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtSettings _jwtSettings;

    public AuthController(
        EmailSenderService emailSenderService,
        IClock clock,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> options)
    {
        _emailService = emailSenderService;
        _clock = clock;
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtSettings = options.Value;
    }

    /// <summary>
    /// Handles user registration by creating a new user, validating the password, 
    /// and sending an email confirmation link.
    /// </summary>
    /// <param name="model">The user's registration details like name, email, and password.</param>
    /// <returns>
    /// Returns 200 (OK) with the confirmation token if successful, 
    /// or 400 (Bad Request) with errors if the registration fails.
    /// </returns>

    // We will also add verion of endpoint into post controller
    [HttpPost("api/v1/Auth/Register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register(
       [FromBody] RegisterModel model
       )
    {
        var validator = new PasswordValidator<ApplicationUser>();
        var now = _clock.GetCurrentInstant();

        var newUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            FirstName = model.FirstName,
            LastName = model.LastName,
            DateOfBirth = model.DateOfBirth,
            UserName = model.UserName,
            Email = model.Email,
        }.SetCreateBySystem(now);

        var checkPassword = await validator.ValidateAsync(_userManager, newUser, model.Password);

        if (!checkPassword.Succeeded)
        {
            ModelState.AddModelError<RegisterModel>(
                x => x.Password, string.Join("\n", checkPassword.Errors.Select(x => x.Description)));
            return ValidationProblem(ModelState);
        }

        // Method with SaveChanges()!
        var result = await _userManager.CreateAsync(newUser);
        // Method with SaveChanges()!
        var pswRslt = await _userManager.AddPasswordAsync(newUser, model.Password);

        var token = string.Empty;
        token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

        return Ok(token);
    }

    /// <summary>
    /// This method lets a user log in by checking their email and password. 
    /// It also makes sure the email is confirmed before logging in.
    /// </summary>
    /// <param name="model">The user's email and password.</param>
    /// <returns>
    /// If the login works, it returns 204 (No Content). If it fails, it shows an error message.
    /// </returns>

    [HttpPost("api/v1/Auth/Login")]
    public async Task<ActionResult> Login([FromBody] LogInModel model)
    {
        var normalizedEmail = model.Email.ToUpperInvariant();
        var user = await _userManager
            .Users
            .SingleOrDefaultAsync(x => x.EmailConfirmed && x.NormalizedEmail == normalizedEmail)
            ;

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "LOGIN_FAILED");
            return ValidationProblem(ModelState);
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "LOGIN_FAILED");
            return ValidationProblem(ModelState);
        }

        var token = GenerateJwtToken(model.Email, user.Id.ToString().ToLowerInvariant());
        return Ok(new { Token = token });
    }
    /// <summary>
    /// unescape token before sending
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost("api/v1/Auth/ValidateToken")]
    public async Task<ActionResult> ValidateToken(
        [FromBody] TokenModel model
        )
    {
        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            ModelState.AddModelError<TokenModel>(x => x.Token, "INVALID_TOKEN");
            return ValidationProblem(ModelState);
        }

        var check = await _userManager.ConfirmEmailAsync(user, model.Token);
        if (!check.Succeeded)
        {
            ModelState.AddModelError<TokenModel>(x => x.Token, "INVALID_TOKEN");
            return ValidationProblem(ModelState);
        }

        return NoContent();
    }
    [AllowAnonymous]
    [HttpGet("api/v1/Account/UserInfo")]
    public async Task<ActionResult<LoggedUserModel>> GetUserInfo()
    {
        if (!User.Identities.Any(x => x.IsAuthenticated))
        {
            return new LoggedUserModel
            {
                id = default,
                name = null,
                isAuthenticated = false,
                isAdmin = false,
            };
        }

        var id = User.GetUserId();
        var user = await _userManager.Users
            .Where(x => x.Id == id)
            .AsNoTracking()
            .SingleAsync();

        var loggedModel = new LoggedUserModel
        {
            id = user.Id,
            name = user.DisplayName,
            isAuthenticated = true,
            isAdmin = false,
        };

        return loggedModel;
    }

    /// <summary>
    /// Logs the user out by ending their session.
    /// </summary>
    /// <returns>
    /// Returns 204 (No Content) when the logout is successful.
    /// </returns>
    [Authorize]
    [HttpPost("api/v1/Auth/Logout")]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    /// <summary>
    /// A test endpoint to check access before and after logging in.
    /// </summary>
    /// <returns>
    /// Returns 200 (OK) with a success message if the endpoint is reached.
    /// </returns>

    [Authorize]
    [HttpGet("api/v1/Auth/TestMeBeforeLoginAndAfter")]
    public ActionResult TestMeBeforeLoginAndAfter()
    {
        return Ok("Succesfully reached endpoint!");
    }
    private string GenerateJwtToken(string username, string id)
    {
        var claims = new List<Claim> { new(ClaimTypes.Name, username), new(ClaimTypes.NameIdentifier, id) };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
