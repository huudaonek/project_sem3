
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoffeeLands.Data;
using System;
using System.Linq;
using Faker;
using Bogus;
using Bogus.DataSets;

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
                new User { Name="David", Email="hahaha@gmail.com", Password="12345"},
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


            var productFaker = new Faker<Product>()
    .RuleFor(p => p.Name, f => f.Commerce.ProductName())
    .RuleFor(p => p.Image, f => $"/customer/images/uploads/anh-{f.Random.Number(1, 5)}.jpg")
    .RuleFor(p => p.Price, f => f.Random.Decimal(1, 1000))
    .RuleFor(p => p.Description, f => f.Lorem.Sentence())
    .RuleFor(p => p.CategoryID, f => f.PickRandom(categories).Id);

            var products = productFaker.Generate(50); // Đổi số lượng sản phẩm cần faker

            foreach (var product in products)
            {
                context.Product.Add(product);
            }

            context.SaveChanges();



        }
    }
}

