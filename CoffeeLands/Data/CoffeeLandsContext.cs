using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoffeeLands.Models;
using Microsoft.EntityFrameworkCore.Migrations;


namespace CoffeeLands.Data
{
    public class CoffeeLandsContext : DbContext
    {
        public CoffeeLandsContext(DbContextOptions<CoffeeLandsContext> options)
            : base(options)
        {
        }
        public DbSet<CoffeeLands.Models.User> User { get; set; } = default!;
        public DbSet<CoffeeLands.Models.Category> Category { get; set; } = default!;
        public DbSet<CoffeeLands.Models.Product> Product { get; set; } = default!;
        //public DbSet<CoffeeLands.Models.Cart> Cart { get; set; } = default!;
        public DbSet<CoffeeLands.Models.Order> Order { get; set; } = default!;
        public DbSet<CoffeeLands.Models.OrderProduct> OrderProduct { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User")
                .HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Category>().ToTable("Category")
                .HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Product>().ToTable("Product")
                .HasIndex(p => p.Name).IsUnique();
            modelBuilder.Entity<Order>().ToTable("Order");
            modelBuilder.Entity<OrderProduct>().ToTable("OrderProduct")
                .HasKey(op => new { op.OrderID, op.ProductID });
            //modelBuilder.Entity<Cart>().ToTable("Cart")
            //    .HasKey(c => new { c.UserID, c.ProductID });


        }
    }
}
