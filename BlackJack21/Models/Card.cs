using System.ComponentModel.DataAnnotations;
namespace BlackJack21.Models
{
    public class Card
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Rank { get; set; }
        [Required]
        public string Suit {  get; set; }

        //FK
        public int HandId { get; set; }

        //Nav Property
        public Hand Hand { get; set; }

    }
}
