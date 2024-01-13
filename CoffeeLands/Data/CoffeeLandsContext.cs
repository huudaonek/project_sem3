using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoffeeLands.Models;

namespace CoffeeLands.Data
{
    public class CoffeeLandsContext : DbContext
    {
        public CoffeeLandsContext (DbContextOptions<CoffeeLandsContext> options)
            : base(options)
        {
        }

        public DbSet<CoffeeLands.Models.Coffee> Coffee { get; set; } = default!;
    }
}
