using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using CoffeeLands.Data;
using CoffeeLands.Helpers;
using System;
using System.Linq;
using Faker;
using Bogus;
using Bogus.DataSets;
using CoffeeLands.ViewModels;

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
                new User { Name="Admin", Email="quannhth2210007@fpt.edu.vn", Password=DataEncryptionExtensions.HashPassword("12345") ,Role="ADMIN", Is_active=true},
                new User { Name="HuuQuan", Email="quan2004@gmail.com", Password=DataEncryptionExtensions.HashPassword("1234"),Role="CUSTOMER", Is_active=true},
                new User { Name="David", Email="hahaha@gmail.com", Password=DataEncryptionExtensions.HashPassword("123"),Role="CUSTOMER"},
                new User { Name="Adam", Email="abc@gmail.com", Password=DataEncryptionExtensions.HashPassword("12"),Role="CUSTOMER"},
                new User { Name="Eva", Email="123@gmail.com", Password=DataEncryptionExtensions.HashPassword("1"),Role="CUSTOMER", Is_active=true},
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
    //.RuleFor(p => p.Name, (f, p) => p.Name.Substring(0, Math.Min(p.Name.Length, 30)))
    .RuleFor(p => p.Image, f => $"/customer/images/uploads/anh_{f.Random.Number(1, 5)}.jpg")
    .RuleFor(p => p.Price, f => f.Random.Decimal(1, 100))
    .RuleFor(p => p.Description, f => f.Lorem.Sentence())
    .RuleFor(p => p.CategoryID, f => f.PickRandom(categories).Id);

            var products = productFaker.Generate(50); // Đổi số lượng sản phẩm cần faker

            foreach (var product in products)
            {
                context.Product.Add(product);
            }
            context.SaveChanges();


            var feedbackFaker = new Faker<Feedback>()
    .RuleFor(p => p.Vote, f => f.Random.Number(1, 5))
    .RuleFor(p => p.imagesFeedback, f => $"/customer/images/feedbacks/anh-{f.Random.Number(1, 5)}.jpg")
    .RuleFor(p => p.Description, f => f.Lorem.Sentence())
    .RuleFor(p => p.UserID, f => f.PickRandom(users).Id)
    .RuleFor(p => p.ProductID, f => f.Random.Number(1, 10));

            var feedbacks = feedbackFaker.Generate(50); 

            foreach (var feedback in feedbacks)
            {
                context.Feedback.Add(feedback);
            }
            context.SaveChanges();

           
        }
    }
}