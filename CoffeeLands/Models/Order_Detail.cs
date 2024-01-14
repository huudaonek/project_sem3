using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoffeeLands.Models
{
	public class Order_Detail
	{
		[RegularExpression(@"^\d+$"), Required, StringLength(10)]
		public string? Qty { get; set; }
		[Range(1, 100), DataType(DataType.Currency)]
		[Column(TypeName = "decimal(18, 2)")]
		public decimal Price { get; set; }

	}
}
