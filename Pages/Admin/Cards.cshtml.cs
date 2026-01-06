using LitackaApi.Auth;
using LitackaApi.Data;
using LitackaApi.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LitackaApi.Pages.Admin;

[Authorize(Roles = $"{Roles.Admin},{Roles.SuperAdmin}")]
public class CardsModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;

    public CardsModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public List<CardStatus> Statuses { get; private set; } = new();
    public List<UserCardRow> Rows { get; private set; } = new();

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    // Založení karty pro uživatele (pokud neexistuje)
    public async Task<IActionResult> OnPostCreateAsync(string userId)
    {
        // Pojistka: user musí existovat
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return NotFound();

        // Pojistka: karta už existuje?
        var existing = await _db.Cards.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId);
        if (existing is not null)
            return RedirectToPage(); // nic nedělat

        var initialStatus = await _db.CardStatuses
            .OrderBy(x => x.Order)
            .FirstOrDefaultAsync();

        if (initialStatus is null)
            return BadRequest("V databázi nejsou žádné CardStatuses. Nejprve je naplň.");

        var card = new Card
        {
            UserId = userId,
            StatusId = initialStatus.Id,
            ExpirationDate = DateOnly.FromDateTime(DateTime.Today).AddYears(3)
        };

        _db.Cards.Add(card);

        try
        {
            await _db.SaveChangesAsync();
            TempData["Message"] = "Karta založena.";
        }
        catch (DbUpdateException)
        {
            // typicky unikátní index na UserId při double-clicku
            TempData["Error"] = "Kartu se nepodařilo založit (možná už existuje).";
        }

        return RedirectToPage();
    }

    // Uložení změn karty
    public async Task<IActionResult> OnPostSaveAsync(SaveCardInput input)
    {
        // základní validace
        var statusExists = await _db.CardStatuses.AnyAsync(s => s.Id == input.StatusId);
        if (!statusExists)
        {
            TempData["Error"] = "Neplatný stav karty.";
            return RedirectToPage();
        }

        var card = await _db.Cards.FirstOrDefaultAsync(x => x.UserId == input.UserId);
        if (card is null)
        {
            TempData["Error"] = "Karta neexistuje. Nejdřív ji založ.";
            return RedirectToPage();
        }

        // DateOnly je ok, jen uložit
        card.ExpirationDate = input.ExpirationDate;
        card.StatusId = input.StatusId;

        await _db.SaveChangesAsync();
        TempData["Message"] = "Uloženo.";

        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Statuses = await _db.CardStatuses
            .AsNoTracking()
            .OrderBy(s => s.Order)
            .ToListAsync();

        // Načteme všechny karty do dictionary pro jednoduchý join v paměti
        var cards = await _db.Cards
            .AsNoTracking()
            .Include(c => c.Status)
            .ToListAsync();

        var cardByUserId = cards.ToDictionary(c => c.UserId, c => c);

        Rows = new List<UserCardRow>();

        foreach (var u in _userManager.Users.OrderBy(u => u.Email))
        {
            cardByUserId.TryGetValue(u.Id, out var card);

            Rows.Add(new UserCardRow
            {
                UserId = u.Id,
                Email = u.Email ?? u.UserName ?? "(unknown)",
                HasCard = card is not null,
                ExpirationDate = card?.ExpirationDate,
                StatusId = card?.StatusId,
                StatusName = card?.Status?.Name,
                CardId = card?.Id
            });
        }
    }

    public class UserCardRow
    {
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        public bool HasCard { get; set; }

        public DateOnly? ExpirationDate { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? CardId { get; set; }
    }

    public class SaveCardInput
    {
        public string UserId { get; set; } = default!;
        public DateOnly ExpirationDate { get; set; }
        public int StatusId { get; set; }
    }
}
