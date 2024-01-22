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
        [Range(1, 100), DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }
        [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(100)]
        public string? Description { get; set; }
        [RegularExpression(@"^[1-9][0-9]*$"), Required, StringLength(3)]
        public string Qty { get; set; }
        public int CategoryID { get; set; }
        public Category Category { get; set; }
        public ICollection<OrderProduct>? OrderProducts { get; set; }
    }
}
