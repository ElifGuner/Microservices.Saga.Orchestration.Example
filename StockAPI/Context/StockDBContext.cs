using Microsoft.EntityFrameworkCore;
using StockAPI.Models;

namespace StockAPI.Context
{
    public class StockDBContext : DbContext
    {
        public DbSet<Stock> Stocks { get; set; }
        public StockDBContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Stock>().HasData(
                new Stock() { Id = 1, ProductId = 1, Count = 200 },
                new Stock() { Id = 2, ProductId = 2, Count = 300},
                new Stock() { Id = 3, ProductId = 3, Count = 50 },
                new Stock() { Id = 4, ProductId = 4, Count = 10 },
                new Stock() { Id = 5, ProductId = 5, Count = 60 }
            );
        }
        
    }
}
