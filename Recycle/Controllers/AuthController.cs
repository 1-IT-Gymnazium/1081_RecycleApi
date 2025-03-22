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
using Recycle.Api.Models.Authorization.PasswordReset;
using Recycle.Api.Services;
using Recycle.Api.Settings;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities.Identity;
using Recycle.Data.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
    private readonly AppDbContext _dbContext;
    private readonly EnviromentSettings _environmentSettings;
    private readonly IApplicationMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class with all required services.
    /// </summary>
    public AuthController(
    EmailSenderService emailSenderService,
        IClock clock,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> options,
        AppDbContext dbContext,
        IOptions<EnviromentSettings> enviromentSettings
        ,

        IApplicationMapper mapper
        )
    {
        _emailService = emailSenderService;
        _clock = clock;
        _signInManager = signInManager;
        _userManager = userManager;
        _jwtSettings = options.Value;
        _dbContext = dbContext;
        _environmentSettings = enviromentSettings.Value;
        _mapper = mapper;
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
        await _userManager.CreateAsync(newUser);
        // Method with SaveChanges()!
        await _userManager.AddPasswordAsync(newUser, model.Password);

        var token = string.Empty;
        token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

        //var url = _environmentSettings.FrontendHostUrl + "/" + _environmentSettings.FrontendConfirmUrl;
        //var escapedToken = Uri.EscapeDataString(token);
        //await _emailService.AddEmail("Registrace", $"<a href=\"{url}?token={escapedToken}&email={newUser.Email}\">Not a scam! Click me</a>", model.Email);
        //var url = $"{_environmentSettings.FrontendHostUrl}/{_environmentSettings.FrontendConfirmUrl}";
        //var escapedToken = Uri.EscapeDataString(token);

        await _emailService.AddEmailToSendAsync(
            model.Email,
            "Confirmation of registration",
            $@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                padding: 20px;
            }}
            .container {{
                max-width: 600px;
                margin: 0 auto;
                background: #ffffff;
                padding: 20px;
                border-radius: 8px;
                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                text-align: center;
            }}
            .button {{
                display: inline-block;
                padding: 10px 20px;
                font-size: 16px;
                color: #fff;
                background-color: #28a745;
                text-decoration: none;
                border-radius: 5px;
                margin-top: 20px;
            }}
            .footer {{
                margin-top: 20px;
                font-size: 12px;
                color: #777;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h2>Confirm of registration</h2>
            <p>Click on the button to verify the email address:</p>
<a href='http://localhost:4200/confirm?token={Uri.EscapeDataString(token)}&email={model.Email}' class='button'>Potvrdit e-mail</a>
<p class='footer'>If you did not register on Recycle!, please ignore this email.</p>
        </div>
    </body>
    </html>"
        );
        return Ok();
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

        var accessToken = GenerateAccessToken(user.Id, model.Email, user.UserName!, _jwtSettings.AccessTokenExpirationInMinutes);
        var refreshToken = await GenerateRefreshTokenAsync(user.Id, _jwtSettings.RefreshTokenExpirationInDays);
        Response.Cookies.Append("RefreshToken", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // For HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays)
        });
        return Ok(new { Token = accessToken });
    }

    /// <summary>
    /// Validates the email confirmation token for user registration.
    /// </summary>
    /// <param name="model">Model containing the token and email to validate.</param>
    /// <returns>
    /// Returns 204 (No Content) if token is valid, or 400 (Bad Request) if token is invalid or expired.
    /// </returns>

    [HttpPost("api/v1/Auth/ValidateToken")]
    public async Task<ActionResult> ValidateToken(
        [FromBody] TokenModel model
        )
    {
        var normalizedMail = model.Email.ToUpperInvariant();
        var user = await _userManager
            .Users
            .SingleOrDefaultAsync(x => !x.EmailConfirmed && x.NormalizedEmail == normalizedMail);

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

    /// <summary>
    /// Retrieves basic information about the currently authenticated user.
    /// </summary>
    /// <returns>
    /// Returns 200 (OK) with user info, or default values if user is not authenticated.
    /// </returns>
    [AllowAnonymous]
    [HttpGet("api/v1/Auth/UserInfo")]
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
            name = user.UserName,
            isAuthenticated = true,
            isAdmin = user.IsAdmin,
        };

        return loggedModel;
    }

    /// <summary>
    /// Generates a new access token and refresh token using the existing refresh token cookie.
    /// </summary>
    /// <returns>
    /// Returns 200 (OK) with the new access token, or 401 (Unauthorized) if refresh token is invalid.
    /// </returns>
    [HttpPost("api/v1/Auth/Refresh")]
    public async Task<IActionResult> RefreshToken()
    {
        if (!Request.Cookies.TryGetValue("RefreshToken", out var incomingToken))
        {
            return Unauthorized(new { Message = "Refresh token not found" });
        }
        var hashedToken = Hash(incomingToken);
        var storedToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == hashedToken);
        if (storedToken == null || storedToken.ExpiresAt < _clock.GetCurrentInstant() || storedToken.RevokedAt != null)
        {
            return Unauthorized(new { Message = "Invalid or expired refresh token" });
        }
        // Generate new access and refresh tokens
        var user = await _dbContext.Users.FindAsync(storedToken.UserId);
        if (user == null)
        {
            return Unauthorized();
        }
        // Generate new tokens
        var newAccessToken = GenerateAccessToken(user.Id, user.Email!, user.UserName!, _jwtSettings.AccessTokenExpirationInMinutes);
        var newRefreshToken = await GenerateRefreshTokenAsync(user.Id, _jwtSettings.RefreshTokenExpirationInDays);
        storedToken.RevokedAt = _clock.GetCurrentInstant();
        await _dbContext.SaveChangesAsync();
        Response.Cookies.Append("RefreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // For HTTPS
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationInDays)
        });
        return Ok(new
        {
            Token = newAccessToken,
        });
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

    /// <summary>
    /// Initiates password reset by generating a reset token and sending a reset email.
    /// </summary>
    /// <param name="model">Model containing the email address.</param>
    /// <returns>
    /// Returns 200 (OK) if email is sent successfully, or 400 (Bad Request) if user is not found.
    /// </returns>
    [HttpPost("api/v1/Auth/ForgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "USER_NOT_FOUND");
            return ValidationProblem(ModelState);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var escapedToken = Uri.EscapeDataString(token);
        var resetUrl = $"{_environmentSettings.FrontendHostUrl}/{_environmentSettings.FrontendResetPasswordUrl}?token={escapedToken}&email={model.Email}";

        await _emailService.AddEmailToSendAsync(
            model.Email,
            "Password Reset Request",
            $@"
    <html>
    <head>
        <style>
            body {{
                font-family: Arial, sans-serif;
                background-color: #f4f4f4;
                padding: 20px;
            }}
            .container {{
                max-width: 600px;
                margin: 0 auto;
                background: #ffffff;
                padding: 20px;
                border-radius: 8px;
                box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                text-align: center;
            }}
            .button {{
                display: inline-block;
                padding: 10px 20px;
                font-size: 16px;
                color: #fff;
                background-color: #dc3545;
                text-decoration: none;
                border-radius: 5px;
                margin-top: 20px;
            }}
            .footer {{
                margin-top: 20px;
                font-size: 12px;
                color: #777;
            }}
        </style>
    </head>
    <body>
        <div class='container'>
            <h2>Password Reset Request</h2>
            <p>If you requested a password reset, click the button below:</p>
            <a href='{resetUrl}' class='button'>Reset Password</a>
            <p class='footer'>If you did not request a password reset, please ignore this email.</p>
        </div>
    </body>
    </html>"
        );

        return Ok(new { message = "Password reset email sent successfully." });
    }

    /// <summary>
    /// Resets the user's password using a valid reset token.
    /// </summary>
    /// <param name="model">Model containing email, reset token, and new password.</param>
    /// <returns>
    /// Returns 200 (OK) on success, or 400 (Bad Request) if token is invalid or user not found.
    /// </returns>
    [HttpPost("api/v1/Auth/ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "USER_NOT_FOUND");
            return ValidationProblem(ModelState);
        }

        var resetResult = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        if (!resetResult.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "RESET_FAILED");
            return ValidationProblem(ModelState);
        }

        return Ok(new { message = "Password reset successful." });
    }

    /// <summary>
    /// Generates a secure refresh token and stores it in the database.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="expirationInDays">How many days until the token expires.</param>
    /// <returns>
    /// Returns the plain refresh token string.
    /// </returns>
    private async Task<string> GenerateRefreshTokenAsync(Guid userId, int expirationInDays)
    {
        var refreshToken = Guid.NewGuid().ToString();
        var data = Request.Headers.UserAgent.ToString();

        var now = _clock.GetCurrentInstant();
        _dbContext.Add(new RefreshToken
        {
            UserId = userId,
            Token = Hash(refreshToken),
            CreatedAt = now,
            ExpiresAt = now.Plus(Duration.FromDays(expirationInDays)),
            RequestInfo = data,
        });
        await _dbContext.SaveChangesAsync();
        return refreshToken;
    }

    /// <summary>
    /// Generates a signed JWT access token for the given user.
    /// </summary>
    /// <param name="userId">User ID to include in token claims.</param>
    /// <param name="email">User's email address.</param>
    /// <param name="username">User's username.</param>
    /// <param name="expirationInMinutes">Token expiration time in minutes.</param>
    /// <returns>
    /// Returns a string representation of the JWT token.
    /// </returns>
    private string GenerateAccessToken(Guid userId, string email, string username, int expirationInMinutes)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString().ToLowerInvariant()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Name, username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(expirationInMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Hashes a given token using SHA256 for secure storage.
    /// </summary>
    /// <param name="token">The token to hash.</param>
    /// <returns>
    /// Returns the Base64-encoded SHA256 hash of the token.
    /// </returns>
    public static string Hash(string token)
    {
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);

    }
}
