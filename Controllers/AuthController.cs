using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LitackaApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AuthController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    /// <summary>
    /// Přihlášení uživatele přes Identity cookie.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Email a heslo jsou povinné.");

        var email = req.Email.Trim().ToLowerInvariant();

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Unauthorized();

        var result = await _signInManager.PasswordSignInAsync(
            userName: user.UserName!,
            password: req.Password,
            isPersistent: req.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
            return Unauthorized();

        return Ok(new { message = "Přihlášeno" });
    }

    /// <summary>
    /// Odhlášení (zrušení Identity cookie).
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Odhlášeno" });
    }

    public sealed record LoginRequest(string Email, string Password, bool RememberMe);
}
