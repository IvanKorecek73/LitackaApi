using LitackaApi.Common;
using LitackaApi.Data;
using Microsoft.EntityFrameworkCore;

namespace LitackaApi.Services.Cards;

public class CardsService : ICardsService
{
    private readonly ApplicationDbContext _db;

    public CardsService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Result<string>> GetCardStateAsync(int cardId)
    {
        try
        {
            var card = await _db.Cards
                .AsNoTracking()
                .Include(c => c.Status)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card is null)
            {
                // todo: log error - replace with proper logging
                Console.WriteLine($"GetCardStateAsync: Karta s číslem {cardId} nenalezena.");
                return Result<string>.NotFound($"Karta s číslem {cardId} nenalezena.");
            }

            var value = card.Status?.Name ?? string.Empty;

            return Result<string>.Success(value);
        }
        catch (Exception ex)
        {
            // TODO: Log exception - replace with proper logging
            Console.WriteLine($"GetCardStateAsync: Error: {ex.Message}");
            return Result<string>.Error(ex.Message);
        }
    }

    public async Task<Result<string>> GetCardValidityAsync(int cardId)
    {
        try
        {
            var card = await _db.Cards
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card is null)
            {
                // TODO: Log error - replace with proper logging
                Console.WriteLine($"GetCardValidityAsync: Karta s číslem {cardId} nenalezena.");
                return Result<string>.NotFound($"Karta s číslem {cardId} nenalezena.");
            }

            var value = card.ExpirationDate.ToString("dd.M.yyyy");
            return Result<string>.Success(value);
        }
        catch (Exception ex)
        {
            // TODO: Log exception - replace with proper logging
            Console.WriteLine($"GetCardValidityAsync: Error: {ex.Message}");
            return Result<string>.Error(ex.Message);
        }
    }
}