using System.ComponentModel.DataAnnotations;

namespace CoffeeLands.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        [Required]
        public int Vote { get; set; }
        public string imagesFeedback { get; set; }
        [Required]
        public string Description { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public User User { get; set; }
        public Product Product { get; set; }
    }
}
