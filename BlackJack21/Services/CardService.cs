using BlackJack21.Data;
using BlackJack21.Models;
using BlackJack21.Services.Abstractions;
using BlackJack21.ViewModels;
using Microsoft.EntityFrameworkCore;


namespace BlackJack21.Services
{
    public class CardService : ICardService
    {
        
        private readonly AppDbContext _db;

        public CardService(AppDbContext appDbContext)
        {
            _db = appDbContext;
        }
        
        
        //Initializes Game and Deck Structure for the game 
        public async Task<int> StartGameAsync()
        {
            //Initialize game
            var newGame = new Game { IsFinished = false };
            var deckHand = new Hand { Type = "Deck", Game = newGame};
            var playerHand = new Hand { Type = "Player", Game = newGame };
            var dealerHand = new Hand { Type = "Dealer", Game = newGame };

            newGame.Hands.Add(deckHand);
            newGame.Hands.Add(playerHand);
            newGame.Hands.Add(dealerHand);

            //Create cards
            string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
            string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

            foreach(var suit in suits)
            {
                foreach(var rank in ranks)
                {
                    deckHand.Cards.Add(new Card { Suit = suit, Rank = rank });
                }
            }

            //Fisher-Yates algorithm for shuffling the deck
            var random = new Random();
            int n = deckHand.Cards.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                var value = deckHand.Cards[k];
                deckHand.Cards[k] = deckHand.Cards[n];
                deckHand.Cards[n] = value;
            }


            _db.Games.Add(newGame);
            await _db.SaveChangesAsync();

            await InitialDrawAsync(newGame.Id);
            return newGame.Id;
        }

        
        public async Task<Card> DrawCardAsync(int GameId, int targetHandId)
        {
            
            //Get the top card from the related deck
            var topCard = await _db.Cards
                .Where(p => p.Hand.GameId == GameId && p.Hand.Type == "Deck")
                .OrderBy(p => p.Id)
                .FirstOrDefaultAsync();

            if (topCard == null)
            {
                throw new InvalidOperationException("The deck is out of cards!");
            }

            //Move the card to the target deck
            topCard.HandId = targetHandId;
            await _db.SaveChangesAsync();
     
            return topCard;
        }
        
        //Helper for Finding Ids
        public async Task<int> FindHandId(int GameId, string handType)
        {
            return await _db.Hands
                .Where(p => p.GameId == GameId && p.Type == handType)
                .Select(p => p.Id)
                .FirstOrDefaultAsync();
        }
    
        //The first sequence of draws that will happen automaticly
        public async Task InitialDrawAsync(int GameId)
        {
            
            //Get Hand Ids
            var playerDeckId = await FindHandId(GameId, "Player");
            var dealerDeckId = await FindHandId(GameId, "Dealer");
            
            //Draw card from the draw deck and put it at the player/dealer hand
            
            var firstcard = await DrawCardAsync(GameId, playerDeckId);
            
            var secondcard = await DrawCardAsync(GameId, dealerDeckId);

            var thirdcard = await DrawCardAsync(GameId, playerDeckId);

            var fourthcard = await DrawCardAsync(GameId, dealerDeckId);

        }
    
        public async Task<GameViewModel> GetGameDetailsAsync(int GameId)
        {
            var dealerHand = await _db.Hands
               .Include(p => p.Cards)
               .FirstOrDefaultAsync(p => p.GameId == GameId && p.Type == "Dealer");

            var playerHand = await _db.Hands
                .Include(p => p.Cards)
                .FirstOrDefaultAsync(p => p.GameId == GameId && p.Type == "Player");

            return new GameViewModel
            {
                playerhand = playerHand,
                dealerhand = dealerHand
            };
       
        }






    }
}











