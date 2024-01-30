using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoffeeLands.Models
{
	public enum OrderStatus
	{
		PENDING, CONFIRMED, SHIPPING, SHIPPED, COMPLETE, CANCEL
	}
	//public class OrderStatus
	//{
	//    public const string PENDING = "";
	//    public const string CONFIRMED = "1";
	//    public const string SHIPPING = "2";
	//    public const string SHIPPED = "3";
	//    public const string COMPLETE = "4";
	//    public const string CANCEL = "5";

	//    private OrderStatus() { }
	//}
	public class OrderProduct
	{
		public int Id { get; set; }
		[RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(30)]
		public string Name { get; set; }
		[Required, StringLength(30)]
		public string Email { get; set; }
		[RegularExpression(@"^0[0-9]{9}$"), Required, StringLength(10)]
		public string Tel { get; set; }
		[RegularExpression(@"^[A-Za-z][a-zA-Z\s]*$"), Required, StringLength(50)]
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


		//private string GetFormattedStatus()
		//{
		//get { return GetFormattedStatus();}
		//    switch (Status)
		//    {
		//        case OrderStatus.PENDING:
		//            return "<span class='text-secondary'>Chờ xác nhận</span>";
		//        case OrderStatus.CONFIRMED:
		//            return "<span class='text-info'>Đã xác nhận</span>";
		//        case OrderStatus.SHIPPING:
		//            return "<span class='text-lightblue'>Đang giao hàng</span>";
		//        case OrderStatus.SHIPPED:
		//            return "<span class='text-pink'>Đã giao hàng</span>";
		//        case OrderStatus.COMPLETE:
		//            return "<span class='text-success'>Hoàn thành</span>";
		//        case OrderStatus.CANCEL:
		//            return "<span class='text-danger'>Huỷ</span>";
		//        default:
		//            return "<span class='text-secondary'>Chờ xác nhận</span>";
		//    }
		//}

		public int UserID { get; set; }

		public User User { get; set; }

		public ICollection<OrderDetail>? OrderDetails { get; set; }
	}
}
