using CoffeeLands.Models;
using System.ComponentModel.DataAnnotations;

namespace CoffeeLands.Models
{
    public class ProductCart
    {
        public Product CartProduct { get; set; }
        public User CartUser { get; set; }
        [RegularExpression(@"^[1-9][0-9]*$"), Required, StringLength(10)]
        public int Qty { get; set; }
    }
}


//[Key]
//public int ProductID { get; set; }
//public int UerID { get; set; }
//public Product CartProduct { get; set; }
//public User UserProduct { get; set; }
//[RegularExpression(@"^[1-9][0-9]*$"), Required, StringLength(10)]
//public int Qty { get; set; }