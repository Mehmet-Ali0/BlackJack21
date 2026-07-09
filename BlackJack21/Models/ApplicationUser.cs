using Microsoft.AspNetCore.Identity;

namespace BlackJack21.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int Balance { get; set; } = 1000;
    
        public List<Game> Games { get; set; } = new List<Game>();
    
    }
}
