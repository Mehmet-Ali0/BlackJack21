using BlackJack21.Data;
using BlackJack21.Models;
using BlackJack21.Services;
using BlackJack21.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackJack21.Controllers
{
    public class BlackJackController : Controller
    {
        private readonly ICardService _cardService;
        private readonly AppDbContext _db;

        public BlackJackController(ICardService cardservice, AppDbContext db) 
        {
            _cardService = cardservice;
            _db = db;
        }


        
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("BlackJack/Start")]
        public async Task<IActionResult> Start()
        {
            int newGameId = await _cardService.StartGameAsync();
            return RedirectToAction("Play", new { id = newGameId });
        }


        //The first deal
        [HttpGet("BlackJack/Play/{id}")]
        public async Task<IActionResult> Play(int id)
        {
            var model = await _cardService.GetGameDetailsAsync(id);

            // THE FIX: Safely grab the Game state from the first hand in your new list!
            bool isGameFinished = model.playerhands.FirstOrDefault()?.Game?.IsFinished ?? false;

            // Natural 21 Check on the Initial Deal
            if (isGameFinished && TempData["GameResult"] == null)
            {
                // Safely calculate using the exact Hand IDs so the DB doesn't get confused
                int playerScore = await _cardService.CalculateScoreByHandIdAsync(model.playerhands[0].Id);
                int dealerScore = await _cardService.CalculateScoreByHandIdAsync(model.dealerhand.Id);

                if (playerScore == 21 && dealerScore == 21) TempData["GameResult"] = "🤝 Double Natural! Tie.";
                else if (playerScore == 21) TempData["GameResult"] = "🎉 NATURAL BLACKJACK! You win!";
                else if (dealerScore == 21) TempData["GameResult"] = "❌ Dealer Natural 21. You lose.";
            }

            return View(model);
        }

        [HttpPost("BlackJack/Hit/{id}")]
        public async Task<IActionResult> Hit(int id)
        {
            //Draw the card
            int currentscore = await _cardService.PlayerHitAsync(id);

            var game = await _db.Games.FindAsync(id);

            //Bust
            if (game != null && game.IsFinished)
            {
                string resultMessage = await _cardService.DealerHitAsync(id);
                TempData["GameResult"] = resultMessage;
            }
           
            //BlackJack force to stand
            else if(currentscore == 21)
            {
                return RedirectToAction("Stand", new { id = id });
            }
            //Show new cards
            return RedirectToAction("Play", new { id = id });
        }


        [HttpPost("BlackJack/Stand/{id}")]
        public async Task<IActionResult> Stand(int id)
        {
            // 1. Fetch the game and all player hands
            var game = await _db.Games
                .Include(p => p.Hands)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (game != null)
            {
                var playerHands = game.Hands
                    .Where(h => h.Type == "Player")
                    .OrderBy(h => h.Id)
                    .ToList();

                int currentIndex = playerHands.FindIndex(h => h.Id == game.ActiveHandId);

                // 2. THE GRACEFUL PIVOT: Is there another hand waiting after this one?
                if (currentIndex != -1 && currentIndex < playerHands.Count - 1)
                {
                    // Yes! Shift the spotlight to the next hand.
                    game.ActiveHandId = playerHands[currentIndex + 1].Id;
                    await _db.SaveChangesAsync();

                    // Do NOT call the dealer. Just reload the page so the player can play Hand 2!
                    return Redirect($"/BlackJack/Play/{id}");
                }
            }

            // 3. If we made it here, there are no more player hands. Dealer's turn!
            string resultMessage = await _cardService.DealerHitAsync(id);
            TempData["GameResult"] = resultMessage;

            return Redirect($"/BlackJack/Play/{id}");
        }



        [HttpPost("BlackJack/Split/{id}")]
        public async Task<IActionResult> Split(int id)
        {
            // Calls the Split logic you wrote earlier
            await _cardService.HandSplit(id);
            return Redirect($"/BlackJack/Play/{id}");
        }








    }








    
    
    
}
