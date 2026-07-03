using BlackJack21.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlackJack21.Controllers
{
    public class BlackJackController : Controller
    {
        private readonly CardService _cardService;
        

        public BlackJackController(CardService cardservice) 
        {
            _cardService = cardservice;
        }


        
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Start()
        {
            int newGameId = await _cardService.StartGameAsync();
            return RedirectToAction("Play", new { id = newGameId });
        }
    
    
    
    
    }
}
