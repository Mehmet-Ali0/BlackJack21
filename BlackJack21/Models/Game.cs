using System.ComponentModel.DataAnnotations;

namespace BlackJack21.Models
{
    public class Game
    {

        [Key]
        public int Id { get; set; }
        [Required]
        public bool IsFinished { get; set; }

        //Navigation property
        public List<Hand> Hands { get; set; } = new List<Hand>();
    
    }
}
