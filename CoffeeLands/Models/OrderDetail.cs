
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace CoffeeLands.Models
{
	public class OrderDetail
	{
		public int OrderProductID { get; set; }
		public int ProductID { get; set; }
		public int Qty { get; set; }
		[DataType(DataType.Currency)]
		[Column(TypeName = "decimal(18, 2)")]
		public decimal Price { get; set; }

		public OrderProduct OrderProduct { get; set; }
		public Product Product { get; set; }
	}
}
