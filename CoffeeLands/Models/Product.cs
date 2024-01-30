using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;


namespace CoffeeLands.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(30)]
        public string Name { get; set; }
       
        public string Image { get; set; }
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(100)]
        public string? Description { get; set; }

        public int CategoryID { get; set; }
        public Category Category { get; set; }
        //public ProductCart? ProductCart { get; set; }
        public ICollection<ProductCart>? ProductCarts { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }

    }
}
