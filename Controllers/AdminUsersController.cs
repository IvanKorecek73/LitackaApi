using LitackaApi.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LitackaApi.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = $"{Roles.Admin},{Roles.SuperAdmin}")]
public class AdminUsersController : ControllerBase
{
    private readonly UserManager<IdentityUser> _users;

    public AdminUsersController(UserManager<IdentityUser> users) => _users = users;

    [HttpGet]
    public IActionResult List()
        => Ok(_users.Users.Select(u => new { u.Id, u.Email, u.UserName }));

    [HttpPost("{userId}/approve")]
    public async Task<IActionResult> Approve(string userId)
    {
        var user = await _users.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        // new-user -> user
        if (await _users.IsInRoleAsync(user, Roles.NewUser))
        {
            await _users.RemoveFromRoleAsync(user, Roles.NewUser);
        }

        if (!await _users.IsInRoleAsync(user, Roles.User))
        {
            await _users.AddToRoleAsync(user, Roles.User);
        }

        return Ok(new { message = "Schváleno (role=user)" });
    }

    [HttpPost("{userId}/set-role")]
    public async Task<IActionResult> SetRole(string userId, [FromBody] SetRoleRequest req)
    {
        if (!Roles.All.Contains(req.Role))
        {
            return BadRequest("Neplatná role.");
        }

        var user = await _users.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        // bezpečnostní pojistka: super-admin roli neměnit běžným adminem (volitelně)
        if (await _users.IsInRoleAsync(user, Roles.SuperAdmin))
        {
            return BadRequest("Nelze měnit super-admina.");
        }

        // vyčistit role a dát jednu cílovou (nebo si to uprav na multi-role)
        var current = await _users.GetRolesAsync(user);
        await _users.RemoveFromRolesAsync(user, current);
        await _users.AddToRoleAsync(user, req.Role);

        return Ok(new { message = $"Role nastavena: {req.Role}" });
    }

    public record SetRoleRequest(string Role);
}
