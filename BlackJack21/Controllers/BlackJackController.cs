using BlackJack21.Data;
using BlackJack21.Models;
using BlackJack21.Services;
using BlackJack21.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
        
        [Authorize]
        [HttpPost("BlackJack/Start")]
        public async Task<IActionResult> Start()
        {
            string userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }

            int newGameId = await _cardService.StartGameAsync(userId);
            return RedirectToAction("Play", new { id = newGameId });
        }


        //The first deal
        [Authorize]
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
        
        [Authorize]
        [HttpPost("BlackJack/Hit/{id}")]
        public async Task<IActionResult> Hit(int id)
        {
            //Draw the card
            int currentscore = await _cardService.PlayerHitAsync(id);

            var game = await _db.Games.FindAsync(id);

            //Dealers Turn You busted.
            if (game != null && game.IsFinished)
            {
                string resultMessage = await _cardService.DealerHitAsync(id);
                TempData["GameResult"] = resultMessage;
            }
           
            //TODO: Bugs out when the second hand hits and gets 21 
            //BlackJack force to stand
            else if(currentscore == 21)
            {
                return RedirectToAction("Stand", new { id = id });
            }
            //Show new cards
            return RedirectToAction("Play", new { id = id });
        }

        [Authorize]
        [HttpPost("BlackJack/Stand/{id}")]
        public async Task<IActionResult> Stand(int id)
        {
            // 1. Fetch the game and all player hands
            var game = await _db.Games
                .Include(p => p.Hands)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (game != null)
            {
                //Get the next active hand
                var nextHand = game.Hands
                        .Where(h => h.Type == "Player" && h.Id > game.ActiveHandId)
                        .OrderBy(h => h.Id)
                        .FirstOrDefault();

                //If one exists set activehand to it and reload the view
                if (nextHand != null)
                {
                    game.ActiveHandId = nextHand.Id;
                    await _db.SaveChangesAsync();
                    return Redirect($"/BlackJack/Play/{id}");
                }
            
            }

            // 3. If we made it here, there are no more player hands. Dealer's turn!
            string resultMessage = await _cardService.DealerHitAsync(id);
            TempData["GameResult"] = resultMessage;

            return Redirect($"/BlackJack/Play/{id}");
        }


        [Authorize]
        [HttpPost("BlackJack/Split/{id}")]
        public async Task<IActionResult> Split(int id)
        {
            // Calls the Split logic you wrote earlier
            await _cardService.HandSplit(id);
            return Redirect($"/BlackJack/Play/{id}");
        }








    }








    
    
    
}
