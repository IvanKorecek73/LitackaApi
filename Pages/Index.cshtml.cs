using LitackaApi.Data;
using LitackaApi.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LitackaApi.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public Card? Card { get; private set; }

    public async Task OnGetAsync()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return;

        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return;

        Card = await _db.Cards
            .Include(c => c.Status)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == user.Id);
    }
}
