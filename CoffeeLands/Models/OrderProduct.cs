using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace CoffeeLands.Models
{
    public class OrderProduct
    {
        public int OrderID { get; set; }
        public int ProductID { get; set; }
        [RegularExpression(@"^[1-9][0-9]*$"), Required, StringLength(10)]
        public string Qty { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        public Order Order { get; set; }
        public Product Product { get; set; }
    }
}
