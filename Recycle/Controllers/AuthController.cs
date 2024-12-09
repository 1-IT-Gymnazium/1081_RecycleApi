using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Recycle.Api.Models.Authorization;
using Recycle.Data.Entities.Identity;
using Recycle.Data.Interfaces;

namespace TwitterEdu.Api.Controllers;
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IClock _clock;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(
        IClock clock,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _clock = clock;
        _signInManager = signInManager;
        _userManager = userManager;
    }

    // We will also add verion of endpoint into post controller
    [HttpPost("api/v1/Auth/Register")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Register(
       [FromBody] SignInModel model
       )
    {
        var validator = new PasswordValidator<ApplicationUser>();
        var now = _clock.GetCurrentInstant();

        var newUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            Username = model.Username,
            Email = model.Email,
        }.SetCreateBySystem(now);

        var checkPassword = await validator.ValidateAsync(_userManager, newUser, model.Password);

        if (!checkPassword.Succeeded)
        {
            ModelState.AddModelError<SignInModel>(
                x => x.Password, string.Join("\n", checkPassword.Errors.Select(x => x.Description)));
            return ValidationProblem(ModelState);
        }

        // Method with SaveChanges()!
        await _userManager.CreateAsync(newUser);
        // Method with SaveChanges()!
        await _userManager.AddPasswordAsync(newUser, model.Password);

        var token = string.Empty;
        token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

        return Ok(token);
    }

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

        var userPrincipal = await _signInManager.CreateUserPrincipalAsync(user);
        await HttpContext.SignInAsync(userPrincipal);

        return NoContent();
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

    [Authorize]
    [HttpPost("api/v1/Auth/Logout")]
    public async Task<ActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpGet("api/v1/Auth/TestMeBeforeLoginAndAfter")]
    public ActionResult TestMeBeforeLoginAndAfter()
    {
        return Ok("Succesfully reached endpoint!");
    }
}
