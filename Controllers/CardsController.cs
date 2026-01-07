using LitackaApi.Auth;
using LitackaApi.Services.Cards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LitackaApi.Controllers;

/// <summary>
/// API pro čtení základních informací o kartě.
/// Určeno výhradně pro interní použití (manager a vyšší).
/// </summary>
[ApiController]
[Route("cards")]
[Authorize(Roles = $"{Roles.Manager},{Roles.Admin},{Roles.SuperAdmin}")]
public class CardsController : ControllerBase
{
    private readonly ICardsService _cardsService;

    public CardsController(ICardsService cardsService)
    {
        _cardsService = cardsService;
    }


    /// <summary>
    /// Vrátí datum expirace karty.
    /// </summary>
    /// <remarks>
    /// Endpoint slouží k ověření platnosti karty.
    /// Vrací pouze datum (bez času) ve formátu <c>dd.M.yyyy</c>.
    /// </remarks>
    /// <param name="cardId">
    /// Interní identifikátor karty (hodnota <c>Id</c> z tabulky <c>Cards</c>).
    /// </param>
    /// <response code="200">Datum expirace karty.</response>
    /// <response code="401">Uživatel není přihlášen.</response>
    /// <response code="403">Uživatel nemá dostatečné oprávnění (vyžaduje manager nebo vyšší).</response>
    /// <response code="404">Karta s daným ID neexistuje.</response>
    [HttpGet("{cardId:int}/validity")]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> GetValidity([FromRoute] int cardId)
    {
        var result = await _cardsService.GetCardValidityAsync(cardId);

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                404 => NotFound(),
                401 => Unauthorized(),
                403 => Forbid(),
                _ => StatusCode(result.StatusCode)
            };
        }

        var value = result.Value!;
        return Content(value, "text/plain");
    }

    /// <summary>
    /// Vrátí aktuální stav karty.
    /// </summary>
    /// <remarks>
    /// Stav odpovídá položce z číselníku <c>CardStatuses</c>
    /// (např. „Aktivní v držení klienta“, „Blokována“, apod.).
    /// </remarks>
    /// <param name="cardId">
    /// Interní identifikátor karty (hodnota <c>Id</c> z tabulky <c>Cards</c>).
    /// </param>
    /// <response code="200">Textový název aktuálního stavu karty.</response>
    /// <response code="401">Uživatel není přihlášen.</response>
    /// <response code="403">Uživatel nemá dostatečné oprávnění (vyžaduje manager nebo vyšší).</response>
    /// <response code="404">Karta s daným ID neexistuje.</response>
    [HttpGet("{cardId:int}/state")]
    [Produces("text/plain")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<ActionResult<string>> GetState([FromRoute] int cardId)
    {
        var result = await _cardsService.GetCardStateAsync(cardId);

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                404 => NotFound(),
                401 => Unauthorized(),
                403 => Forbid(),
                _ => StatusCode(result.StatusCode)
            };
        }

        var value = result.Value!;
        return Content(value, "text/plain");
    }
}
