using BlackJack21.Models;
using BlackJack21.ViewModels;

namespace BlackJack21.Services.Abstractions
{
    public interface ICardService
    {
        Task<int> StartGameAsync(string userId);

       Task<Card> DrawCardAsync(int GameId, int targetHandId);

       Task<int> FindHandId(int GameId, string handType);

       Task InitialDrawAsync(int GameId);
       Task<GameViewModel> GetGameDetailsAsync(int GameId);
       Task<int> PlayerHitAsync(int gameId);
       Task<string> DealerHitAsync(int GameId);
        Task HandSplit(int GameId);
        Task<int> CalculateScoreAsync(int GameId, string HandType);
        Task<int> CalculateScoreByHandIdAsync(int handId);
    }
}
