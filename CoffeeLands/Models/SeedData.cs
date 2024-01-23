
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoffeeLands.Data;
using System;
using System.Linq;

namespace CoffeeLands.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new CoffeeLandsContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<CoffeeLandsContext>>()))
        {
            // Look for any movies.
            if (context.User.Any())
            {
                return;   // db has been seeded
            }

            var users = new User[]
            {
                new User { Name="Admin", Email="quannhth2210007@fpt.edu.vn", Password="12345", Role="ADMIN"},
                new User { Name="HuuQuan", Email="familyquan2004@gmail.com", Password="12345"},
                new User { Name="David", Email="familyquan2004@gmail.com", Password="12345"},
                new User { Name="Adam", Email="abc@gmail.com", Password="12345"},
                new User { Name="Eva", Email="123@gmail.com", Password="12345"},
            };
            foreach (User u in users)
            {
                context.User.Add(u);
            }
            context.SaveChanges();


            var categories = new Category[]
            {
                new Category { Name="Fruit Coffee"},
                new Category { Name="Animal Coffee"},
                new Category { Name="Coffee"},
            };
            foreach (Category c in categories)
            {
                context.Category.Add(c);
            }
            context.SaveChanges();

            var products = new Product[]
            {
                new Product { Name = "Bee Coffee", Price = 7 , Description = "Description Bee Coffee",
                Qty = "12", CategoryID = categories.Single(c => c.Name == "Animal Coffee").Id},
                new Product { Name = "Weasel Coffee", Price = 3, Description = "Description Weasel Coffee",
                Qty = "13", CategoryID = categories.Single(c => c.Name == "Animal Coffee").Id},
                new Product { Name = "Apple Coffee", Price = 2, Description = "Description Apple Coffee",
                Qty = "14", CategoryID = categories.Single(c => c.Name == "Fruit Coffee").Id},
                new Product { Name = "Phin Coffee", Price = 10, Description = "Description Phin Coffee",
                Qty = "15", CategoryID = categories.Single(c => c.Name == "Coffee").Id},
                new Product { Name = "Milk Coffee", Price = 10, Description = "Description Milk Coffee",
                Qty = "20", CategoryID = categories.Single(c => c.Name == "Coffee").Id},
            };
            foreach (Product p in products)
            {
                context.Product.Add(p);
            }
            context.SaveChanges();


            //var carts = new Cart[]
            //{
            //    new Cart { UserID = users.Single(c => c.Name == "Admin").Id, ProductID = products.Single(p => p.Name == "Bee Coffee").Id, BuyQty=2 },
            //    new Cart { UserID = users.Single(c => c.Name == "Admin").Id, ProductID = products.Single(p => p.Name == "Apple Coffee").Id, BuyQty=3 },
            //    new Cart { UserID = users.Single(c => c.Name == "HuuQuan").Id, ProductID = products.Single(p => p.Name == "Weasel Coffee").Id, BuyQty=1 },
            //    new Cart { UserID = users.Single(c => c.Name == "David").Id, ProductID = products.Single(p => p.Name == "Apple Coffee").Id, BuyQty=3 },
            //    new Cart { UserID = users.Single(c => c.Name == "Adam").Id, ProductID = products.Single(p => p.Name == "Phin Coffee").Id, BuyQty=8 },
            //    new Cart { UserID = users.Single(c => c.Name == "Eva").Id, ProductID = products.Single(p => p.Name == "Milk Coffee").Id, BuyQty=5 }
            //};
            //foreach (Cart c in carts)
            //{
            //    context.Cart.Add(c);
            //}
            //context.SaveChanges();

            var orders = new Order[]
            {
                new Order { Name = "Huu Quan", Email="quannhth2210007@fpt.edu.vn", Tel="0123456789", Address="ABC-1A", Grand_total=111,
                    Shipping_method="Free_shipping", Payment_method="COD", Is_paid=true, Status="Da Thanh Toan", UserID = users.Single(c => c.Name == "Admin").Id},
                new Order { Name = "Hihi", Email="familyquan2004@gmail.com", Tel="0987654321", Address="8A-Ton that thuyet", Grand_total=200,
                    Shipping_method="Express", Payment_method="COD", Is_paid=true, Status="Da Thanh Toan", UserID = users.Single(c => c.Name == "HuuQuan").Id},
                new Order { Name = "Huu Quan", Email="quannhth2210007@fpt.edu.vn", Tel="0123456789", Address="ABC-1A", Grand_total=120,
                    Shipping_method="Free_shipping", Payment_method="COD", Is_paid=false, Status="Chua Thanh Toan", UserID = users.Single(c => c.Name == "Admin").Id},

            };
            foreach (Order o in orders)
            {
                context.Order.Add(o);
            }
            context.SaveChanges();


            var order_products = new OrderProduct[]
            {
                new OrderProduct { OrderID=orders.Single(o => o.Id == 1).Id ,ProductID=products.Single(p => p.Name == "Bee Coffee").Id, Qty="2", Price=24},
                new OrderProduct { OrderID=orders.Single(o => o.Id == 1).Id ,ProductID=products.Single(p => p.Name == "Apple Coffee").Id, Qty="2", Price=26},
                new OrderProduct { OrderID=orders.Single(o => o.Id == 3).Id ,ProductID=products.Single(p => p.Name == "Milk Coffee").Id, Qty="2", Price=40},
            };
            foreach (OrderProduct op in order_products)
            {
                context.OrderProduct.Add(op);
            }
            context.SaveChanges();



        }
    }
}

