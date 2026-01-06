using LitackaApi.Common;

namespace LitackaApi.Services.Cards;

public interface ICardsService
{
    Task<Result<string>> GetCardStateAsync(int cardId);
    Task<Result<string>> GetCardValidityAsync(int cardId);
}