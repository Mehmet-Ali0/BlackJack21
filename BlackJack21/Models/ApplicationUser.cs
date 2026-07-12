using Microsoft.AspNetCore.Identity;

namespace BlackJack21.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int Balance { get; set; } = 1000;

        public int? TotalMoneyWon { get; set; } = 0;

        public int? TotalMoneyLost { get; set; } = 0;

        public List<Game> Games { get; set; } = new List<Game>();
    
    }
}
