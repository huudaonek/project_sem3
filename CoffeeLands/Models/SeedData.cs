
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
            if (context.Product.Any())
            {
                return;   // DB has been seeded
            }
            context.Product.AddRange(
                new Product
                {
                    Name = "Bee Coffee",
                    Type = "Coffe",
                    Price = 7.99M,
                    Origin = "VIET NAM",
                    Description = "Description Bee Coffee",
                    Status = "Con hang",
                    Qty = "12",
                },
                new Product
                {
                    Name = "Weasel Coffee",
                    Type = "Coffe",
                    Price = 1.11M,
                    Origin = "AMERICA",
                    Description = "Description Weasel Coffee",
                    Status = "Con hang",
                    Qty = "12",
                },
                new Product
                {
                    Name = "Apple Coffee",
                    Type = "Coffe",
                    Price = 2.22M,
                    Origin = "Chinese",
                    Description = "Description Apple Coffee",
                    Status = "Con hang",
                    Qty = "12",
                },
                new Product
                {
                    Name = "Phin Coffee",
                    Type = "Coffe",
                    Price = 10.10M,
                    Origin = "VIET NAM",
                    Description = "Description Phin Coffee",
                    Status = "Con hang",
                    Qty = "12",
                }
            );
            context.SaveChanges();
        }
    }
}

