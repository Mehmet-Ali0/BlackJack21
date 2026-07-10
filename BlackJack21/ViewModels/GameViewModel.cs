using BlackJack21.Models;

namespace BlackJack21.ViewModels
{
    public class GameViewModel
    {
        public Hand playerhand { get; set; }
        public int? ActiveHandId { get; set; }
        public Hand dealerhand { get; set; }
        public int GameId { get; set; }
        public List<Hand> playerhands { get; set; }

        public int Balance { get; set; }

        public int Bet {  get; set; }

    }
}
