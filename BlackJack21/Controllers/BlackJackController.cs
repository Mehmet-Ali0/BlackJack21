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

        [HttpPost]
        public async Task<IActionResult> Start()
        {
            int newGameId = await _cardService.StartGameAsync();
            return RedirectToAction("Play", new { id = newGameId });
        }

        
        //The first deal
        [HttpGet]
        public async Task<IActionResult> Play(int id)
        {
           
            var model = await _cardService.GetGameDetailsAsync(id);

            return View(model);
        }
    
    
    
    
    
    
    }








    
    
    
}
