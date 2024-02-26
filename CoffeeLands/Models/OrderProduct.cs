using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoffeeLands.Models
{
	public enum OrderStatus
	{
		PENDING, CONFIRMED, SHIPPING, SHIPPED, COMPLETE, CANCEL
	}

	public class OrderProduct
	{
		public int Id { get; set; }
		[RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(30)]
		public string Name { get; set; }
		[Required, StringLength(30)]
		public string Email { get; set; }
		[RegularExpression(@"^0[0-9]{9}$"), Required, StringLength(10)]
		public string Tel { get; set; }
		[RegularExpression(@"^[A-Za-z][a-zA-Z\s]*$"), Required, StringLength(100)]
		public string Address { get; set; }

		[DataType(DataType.Currency)]
		[Column(TypeName = "decimal(18, 2)")]
		public decimal Grand_total { get; set; }
		[Required]
		public string Shipping_method { get; set; }
		[Required]
		public string Payment_method { get; set; }
		public bool Is_paid { get; set; }

		public OrderStatus Status { get; set; }

		public int UserID { get; set; }

		public User User { get; set; }

		public ICollection<OrderDetail>? OrderDetails { get; set; }
	}
}
