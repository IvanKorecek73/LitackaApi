using LitackaApi.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LitackaApi.Pages.Admin;

[Authorize(Roles = $"{Roles.Admin},{Roles.SuperAdmin}")]
public class UsersModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;

    public UsersModel(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public List<UserRow> Users { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Users = [];

        foreach (var user in _userManager.Users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            Users.Add(new UserRow
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? "(unknown)",
                Roles = roles.ToList()
            });
        }
    }

    public async Task<IActionResult> OnPostApproveAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        // new-user -> user
        if (await _userManager.IsInRoleAsync(user, Roles.NewUser))
        {
            await _userManager.RemoveFromRoleAsync(user, Roles.NewUser);
        }

        if (!await _userManager.IsInRoleAsync(user, Roles.User))
        {
            await _userManager.AddToRoleAsync(user, Roles.User);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        // bezpečnostní pojistka
        if (await _userManager.IsInRoleAsync(user, Roles.SuperAdmin))
        {
            return BadRequest("Super-admina nelze smazat.");
        }

        await _userManager.DeleteAsync(user);
        return RedirectToPage();
    }

    public class UserRow
    {
        public string Id { get; init; } = default!;
        public string Email { get; init; } = default!;
        public List<string> Roles { get; init; } = [];
    }
}
