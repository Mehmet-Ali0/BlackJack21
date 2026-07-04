using BlackJack21.Data;
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
           
            if (model.playerhand.Game.IsFinished && TempData["GameResult"] == null)
            {
                int playerScore = await _cardService.CalculateScoreAsync(id, "Player");
                int dealerScore = await _cardService.CalculateScoreAsync(id, "Dealer");

                if (playerScore == 21 && dealerScore == 21)
                {
                    TempData["GameResult"] = "Tie.";
                }
                else if (playerScore == 21)
                {
                    TempData["GameResult"] = "You win!";
                }
                else if (dealerScore == 21)
                {
                    TempData["GameResult"] = "Dealer Wins";
                }
            }
            return View(model);
        }

        [HttpPost("BlackJack/Hit/{id}")]
        public async Task<IActionResult> Hit(int id)
        {
            //Draw the card
            int currentscore = await _cardService.PlayerHitAsync(id);
            
            //Bust
            if(currentscore > 21)
            {
                TempData["GameResult"] = "💥 Bust! You went over 21. Dealer Wins.";
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
            string resultMessage = await _cardService.DealerHitAsync(id);
            TempData["GameResult"] = resultMessage;
            return Redirect($"/BlackJack/Play/{id}");
        }










    }








    
    
    
}
