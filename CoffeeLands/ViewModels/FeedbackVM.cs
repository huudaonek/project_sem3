using CoffeeLands.Models;
using System.ComponentModel.DataAnnotations;

namespace CoffeeLands.ViewModels
{
    public class FeedbackVM
    {
        public int Id { get; set; }
        [Required]
        public int Vote { get; set; }
        public string imagesFeedback { get; set; }
        [Required]
        public string Description { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
    }
}
