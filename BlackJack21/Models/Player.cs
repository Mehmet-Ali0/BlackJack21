using System.ComponentModel.DataAnnotations;
namespace BlackJack21.Models
{
    public class Player
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Chips { get; set; }

        //Nav Property
        public List<Hand> Hands { get; set; } = new List<Hand>();

    
    
    }
}
