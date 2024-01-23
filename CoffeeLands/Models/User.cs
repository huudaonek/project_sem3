using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoffeeLands.Models
{
	public class User
	{
		public int Id { get; set; }
		[Required, StringLength(30)]
		public string Name { get; set; }
		[Required, StringLength(30)]
		public string Email { get; set; }
		[Required, StringLength(30)]
		public string Password { get; set; }
		[StringLength(30)]
		public string? Role { get; set; }
        //public Cart? Cart { get; set; }
        public ICollection<Order>? Orders { get; set; }
    }
}
