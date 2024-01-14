using System.ComponentModel.DataAnnotations;

namespace CoffeeLands.Models
{
	public class Category
	{
		public int Id { get; set; }
		[RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(30)]
		public string? Name { get; set; }
		public string? Slug { get; set; }
	}
}
