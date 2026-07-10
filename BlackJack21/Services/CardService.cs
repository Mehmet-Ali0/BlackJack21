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
        public async Task<int> StartGameAsync(string userId)
        {
            //Initialize game
            var newGame = new Game { IsFinished = false, UserId = userId };
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

            newGame.ActiveHandId = playerHand.Id;
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

            var game = await _db.Games.FindAsync(GameId);

            // If we are looking for the player, and there is an active split hand, return that one
            if (handType == "Player" && game.ActiveHandId.HasValue)
            {
                return game.ActiveHandId.Value;
            }

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
            


            //Natural 21 Case
            int initialPlayerScore = await CalculateScoreAsync(GameId, "Player");
            int initialDealerScore = await CalculateScoreAsync(GameId, "Dealer");

            if (initialPlayerScore == 21 || initialDealerScore == 21)
            {
                var game = await _db.Games.FindAsync(GameId);
                if (game != null)
                {
                    game.IsFinished = true; // Lock the game instantly
                    await _db.SaveChangesAsync();
                }
            }

        }

        //Sending the hands to the viewmodel
        public async Task<GameViewModel> GetGameDetailsAsync(int GameId, string UserId)
        {
            var dealerHand = await _db.Hands
               .Include(p => p.Cards)
               .Include(p => p.Game)
               .FirstOrDefaultAsync(p => p.GameId == GameId && p.Type == "Dealer");

            // Fetch ALL player hands as a list
            var playerHands = await _db.Hands
                .Include(p => p.Cards)
                .Include(p => p.Game)
                .Where(p => p.GameId == GameId && p.Type == "Player")
                .OrderBy(p => p.Id)
                .ToListAsync();

            //Get player balance
            var balance = await _db.Users
                .Where(p => p.Id == UserId)
                .Select(p => p.Balance)
                .FirstOrDefaultAsync();



            var game = await _db.Games.FindAsync(GameId);

            return new GameViewModel
            {
                GameId = GameId,
                ActiveHandId = game?.ActiveHandId,
                playerhands = playerHands,
                dealerhand = dealerHand,
                Balance = balance
            };
        }

        public async Task<int> CalculateScoreAsync(int GameId, string HandType)
        {

            int targetHandId = await FindHandId(GameId, HandType);

            var hand = await _db.Hands
                .Include(p => p.Cards)
                .FirstOrDefaultAsync(p => p.Id == targetHandId);

            if (hand == null) return 0;

            int score = 0;
            int aceCount = 0;
           
            //Calculate card scores
            foreach(var card in hand.Cards)
            {
                if(card.Rank == "Jack" || card.Rank == "Queen" || card.Rank == "King")
                {
                    score = score + 10;
                }
                else if(card.Rank == "Ace")
                {
                    score += 11;
                    aceCount++;
                }
                else
                {
                    score += int.Parse(card.Rank);
                }
            }
            
            //Handling Ace condition as both 11 and 1
            while(score > 21 && aceCount > 0)
            {
                score = score - 10;
                aceCount--;
            }

            return score; 
        }

        public async Task<int> CalculateScoreByHandIdAsync(int handId)
        {
            var hand = await _db.Hands
                .Include(p => p.Cards)
                .FirstOrDefaultAsync(p => p.Id == handId);

            if (hand == null) return 0;

            int score = 0;
            int aceCount = 0;

            foreach (var card in hand.Cards)
            {
                if (card.Rank == "Jack" || card.Rank == "Queen" || card.Rank == "King")
                    score += 10;
                else if (card.Rank == "Ace")
                {
                    score += 11;
                    aceCount++;
                }
                else
                    score += int.Parse(card.Rank);
            }

            while (score > 21 && aceCount > 0)
            {
                score -= 10;
                aceCount--;
            }

            return score;
        }

        //☺ Add the chip Stuff here for calculation
        public async Task<int> PlayerHitAsync(int GameId)
        {
            //Get active handId
            var playerHandId = await FindHandId(GameId, "Player");
            
            //Draw a card from the deck to the player hand
            var card = await DrawCardAsync(GameId, playerHandId);
            
            //Get the score
            var score = await CalculateScoreAsync(GameId, "Player");

            if(score > 21)
            {
                //Fetch the game with the list of hands
                var game = await _db.Games
                    .Include(p => p.Hands)
                    .FirstOrDefaultAsync(p => p.Id == GameId);

                //Safety Check
                if(game != null)
                {
                    //Filter the hands as [0] as the playerhand and [1] as the splithand
                    var playerHands = game.Hands
                        .Where(h => h.Type == "Player")
                        .OrderBy(h => h.Id)
                        .ToList();
                    
                    //Do we have a split and Did the first hand bust
                    if(playerHands.Count > 1 && game.ActiveHandId == playerHands[0].Id)
                    {
                        //Update pointer to the next hand
                        game.ActiveHandId = playerHands[1].Id;
                    }
                    //Player didint split or busted the second hand
                    else
                    {
                        game.IsFinished = true;
                    }       
                    await _db.SaveChangesAsync();
                }
            }
            return score;
        }


        //☺ Add the chip Stuff here for calculation
        public async Task<string> DealerHitAsync(int GameId)
        {
            //Get dealersHand Id and Score
            var dealerHandId = await FindHandId(GameId, "Dealer");
            var dealerScore = await CalculateScoreAsync(GameId, "Dealer");

            //If score is lower then 17 keep drawing cards
            while(dealerScore < 17)
            {
                var card = await DrawCardAsync(GameId, dealerHandId);
                dealerScore = await CalculateScoreAsync(GameId, "Dealer");
            }

            //Get game info
            var game = await _db.Games
            .Include(p => p.Hands)
            .FirstOrDefaultAsync(p => p.Id == GameId);

            //Set game to finished after drawing finishes
            if (game != null)
            {
                game.IsFinished = true;
                await _db.SaveChangesAsync();
            }

            //Filter hands as [0] for player and [1] for split
            var playerHands = game.Hands
                .Where(h => h.Type == "Player")
                .OrderBy(h => h.Id)
                .ToList();

            //If split didnt happen 
            if (playerHands.Count == 1)
            {
                
                int playerScore = await CalculateScoreByHandIdAsync(playerHands[0].Id);

                if (playerScore > 21) return "Bust! You went over 21.";
                if (dealerScore > 21) return "Dealer Busts! You Win!";
                if (playerScore > dealerScore) return "You Win!";
                if (dealerScore > playerScore) return "Dealer Wins.";
                return "🤝 It's a tie.";
            }

            string finalResultMessage = "";

            //Iterate through the hands and consturct the message
            for (int i = 0; i < playerHands.Count; i++)
            {
                int playerScore = await CalculateScoreByHandIdAsync(playerHands[i].Id);
                int handNum = i + 1;

                finalResultMessage += $"Hand {handNum}: ";

                if (playerScore > 21) finalResultMessage += "Bust 💥 | ";
                else if (dealerScore > 21) finalResultMessage += "Win 🎉 | ";
                else if (playerScore > dealerScore) finalResultMessage += "Win 🎉 | ";
                else if (dealerScore > playerScore) finalResultMessage += "Lose ❌ | ";
                else finalResultMessage += "Push 🤝 | ";
            }

            return finalResultMessage.TrimEnd(' ', '|');
       
        }

        public async Task HandSplit(int GameId)
        {
            //Get the original handId for Drawing cards from the deck
            var originalHandId = await FindHandId(GameId, "Player");
            
            //Create the split Hand
            var splitHand = new Hand { Type = "Player", GameId = GameId };
            _db.Hands.Add(splitHand);

            //Put one of the cards from the orignal hand to the split hand
            var topCard = await _db.Cards
                .Where(p => p.Hand.GameId == GameId && p.Hand.Type == "Player")
                .OrderBy(p => p.Id)
                .FirstOrDefaultAsync();
            
            topCard.Hand = splitHand;

            //Save it for the id initialization
            await _db.SaveChangesAsync();

            //Draw other cards to the related hands
            var card1 = await DrawCardAsync(GameId, originalHandId);
            var card2 = await DrawCardAsync(GameId, splitHand.Id);

            //Find the Game
            var game = await _db.Games.FirstOrDefaultAsync(p => p.Id == GameId);
           
            //Set active hand
            game.ActiveHandId = originalHandId;

            await _db.SaveChangesAsync();

        }




    }
}











