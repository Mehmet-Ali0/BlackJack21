using System.Security.Claims;
using BlackJack21.Data;
using BlackJack21.Models;
using BlackJack21.Services;
using BlackJack21.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlackJack21.Controllers
{
    public class BlackJackController : Controller
    {
        private readonly ICardService _cardService;
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public BlackJackController(ICardService cardservice, AppDbContext db, UserManager<ApplicationUser> userManager) 
        {
            _cardService = cardservice;
            _db = db;
            _userManager = userManager;
        }


        [HttpGet]
        [Route("/")]
        public async Task<IActionResult> Index(int order)
        {
            //1 = Net Gain, 2 = TotalWon, 3 = TotalLost
            var query = _db.Users.AsQueryable();

            
            if (order == 2)
            {
                query = query.OrderByDescending(p => p.TotalMoneyWon);
            }
            else if (order == 3)
            {
                query = query.OrderByDescending(p => p.TotalMoneyLost);
            }
            else 
            {
                query = query.OrderByDescending(p => p.TotalMoneyWon - p.TotalMoneyLost);
            }

            
            var sortedList = await query.Take(5).ToListAsync();

            return View(sortedList);

        }
        
        [Authorize]
        [HttpPost("BlackJack/Start")]
        public async Task<IActionResult> Start(int Bet)
        {
            string userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Index");
            }

            int newGameId = await _cardService.StartGameAsync(userId);
            return RedirectToAction("Play", new { id = newGameId, Bet = Bet });
        }


        //The first deal
        [Authorize]
        [HttpGet("BlackJack/Play/{id}")]
        public async Task<IActionResult> Play(int id,int Bet)
        {
            //Get user Id
            string UserId = _userManager.GetUserId(User);
            
            //Get the user itself
            var user = await _userManager.FindByIdAsync(UserId);
            
            var model = await _cardService.GetGameDetailsAsync(id,UserId,Bet);

            // THE FIX: Safely grab the Game state from the first hand in your new list!
            bool isGameFinished = model.playerhands.FirstOrDefault()?.Game?.IsFinished ?? false;

            // Natural 21 Check on the Initial Deal
            if (isGameFinished && TempData["GameResult"] == null)
            {
                // Safely calculate using the exact Hand IDs so the DB doesn't get confused
                int playerScore = await _cardService.CalculateScoreByHandIdAsync(model.playerhands[0].Id);
                int dealerScore = await _cardService.CalculateScoreByHandIdAsync(model.dealerhand.Id);

                bool balanceChanged = false;

                if (playerScore == 21 && dealerScore == 21)
                {
                    TempData["GameResult"] = "🤝 Double Natural! Tie.";
                }
                else if (playerScore == 21)
                {
                    user.Balance = user.Balance + Bet * 2;
                    user.TotalMoneyWon = user.TotalMoneyWon + Bet;
                    TempData["GameResult"] = "🎉 NATURAL BLACKJACK! You win!";
                    balanceChanged = true;
                }
                else if (dealerScore == 21)
                {
                    user.Balance = user.Balance - Bet;
                    user.TotalMoneyLost = user.TotalMoneyLost + Bet;
                    TempData["GameResult"] = "❌ Dealer Natural 21. You lose.";
                    balanceChanged = true;

                }

                if (balanceChanged)
                {
                    await _userManager.UpdateAsync(user);
                    model.Balance = user.Balance;
                }
            
            
            }

            return View(model);
        }
        
        [Authorize]
        [HttpPost("BlackJack/Hit/{id}")]
        public async Task<IActionResult> Hit(int id, int Bet)
        {
            //Draw the card
            int currentscore = await _cardService.PlayerHitAsync(id,Bet);

            var game = await _db.Games.FindAsync(id);

            //Dealers Turn You busted.
            if (game != null && game.IsFinished)
            {
                string resultMessage = await _cardService.DealerHitAsync(id,Bet);
                TempData["GameResult"] = resultMessage;
            }
           
            //TODO: Bugs out when the second hand hits and gets 21 
            //BlackJack force to stand
            else if(currentscore == 21)
            {
                return RedirectToAction("Stand", new { id = id , Bet = Bet});
            }
            //Show new cards
            return RedirectToAction("Play", new { id = id, Bet = Bet });
        }

        [Authorize]
        [HttpPost("BlackJack/Stand/{id}")]
        public async Task<IActionResult> Stand(int id, int Bet)
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
            string resultMessage = await _cardService.DealerHitAsync(id,Bet);
            TempData["GameResult"] = resultMessage;

            return RedirectToAction("Play", new { id = id, Bet = Bet });
            
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
