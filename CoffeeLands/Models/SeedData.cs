
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
            if (context.Category.Any())
            {
                return;   // DB has been seeded
            }

            var categories = new Category[]
            {
                new Category { Name="Coffee Fruit"},
                new Category { Name="Coffee Animal"},
                new Category { Name="Coffee"},
            };
            foreach (Category c in categories)
            {
                context.Category.Add(c);
            }
            context.SaveChanges();

            var products = new Product[]
            {
                new Product { Name = "Bee Coffee", Price = 7.99M, Description = "Description Bee Coffee",
                Qty = "12", CategoryID = categories.Single(c => c.Name == "Coffee Animal").Id},
                new Product { Name = "Weasel Coffee", Price = 1.11M, Description = "Description Weasel Coffee",
                Qty = "12", CategoryID = categories.Single(c => c.Name == "Coffee Animal").Id},
                new Product { Name = "Apple Coffee", Price = 2.22M, Description = "Description Apple Coffee",
                Qty = "12", CategoryID = categories.Single(c => c.Name == "Coffee Fruit").Id},
                new Product { Name = "Phin Coffee", Price = 10.10M, Description = "Description Phin Coffee",
                Qty = "12", CategoryID = categories.Single(c => c.Name == "Coffee").Id},
            };
            foreach (Product p in products)
            {
                context.Product.Add(p);
            }
            context.SaveChanges();


            //context.Product.AddRange(
            //    new Product
            //    {
            //        Name = "Bee Coffee",
            //        Price = 7.99M,
            //        Description = "Description Bee Coffee",
            //        Qty = "12",
            //        CategoryID = categories.Single(c => c.Name == "Coffee Animal").Id
            //    },
            //    new Product
            //    {
            //        Name = "Weasel Coffee",
            //        Price = 1.11M,
            //        Description = "Description Weasel Coffee",
            //        Qty = "12",
            //        CategoryID = categories.Single(c => c.Name == "Coffee Animal").Id
            //    },
            //    new Product
            //    {
            //        Name = "Apple Coffee",
            //        Price = 2.22M,
            //        Description = "Description Apple Coffee",
            //        Qty = "12",
            //        CategoryID = categories.Single(c => c.Name == "Coffee Fruit").Id
            //    },
            //    new Product
            //    {
            //        Name = "Phin Coffee",
            //        Price = 10.10M,
            //        Description = "Description Phin Coffee",
            //        Qty = "12",
            //        CategoryID = categories.Single(c => c.Name == "Coffee").Id
            //    }
            //);
            //context.SaveChanges();
        }
    }
}

