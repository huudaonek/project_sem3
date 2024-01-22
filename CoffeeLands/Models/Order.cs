using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CoffeeLands.Models
{
	public class Order
	{
		public int Id { get; set; }
		[RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(30)]
		public string Name { get; set; }
		[RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(30)]
		public string Email { get; set; }
		[RegularExpression(@"^\d+$"), Required, StringLength(10)]
		public string Tel { get; set; }
		[RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(50)]
		public string Address { get; set; }
		[RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(30)]
		public string Status { get; set; }
		[Range(1, 100), DataType(DataType.Currency)]
		[Column(TypeName = "decimal(18, 2)")]
		public decimal Grand_total { get; set; }
		[RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(30)]
		public string Shipping_method { get; set; }
		[RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(30)]
		public string Payment_method { get; set; }
        public int UserID { get; set; }

        public User User { get; set; }

        public OrderProduct OrderProduct { get; set; }
    }
}
