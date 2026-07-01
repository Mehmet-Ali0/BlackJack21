using System.ComponentModel.DataAnnotations;
namespace BlackJack21.Models
{
    public class Hand
    {

        [Key]
        public int Id { get; set; }
        [Required]
        public string Type { get; set; }

        //FK 
        public int GameId { get; set; }
        public int? PlayerId { get; set; }

        //Nav property
        public Game Game { get; set; }
        public Player? Player { get; set; }
        public List<Card> Cards { get; set; } = new List<Card>();
    
    
    }
}
