using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoffeeLands.Models
{
	public class User
	{
		public int Id { get; set; }
		[Required, StringLength(30)]
		public string Name { get; set; }
		[Required, StringLength(50)]
		public string Email { get; set; }
		[DataType(DataType.Password),Required]
		public string Password { get; set; }
        [Required]
        public string Role { get; set; }
		public bool Is_active { get; set; }
		public ICollection<OrderProduct>? OrderProducts { get; set; }
        public ICollection<Feedback>? Feedbacks { get; set; }
    }
}
