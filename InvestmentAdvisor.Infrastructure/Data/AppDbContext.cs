using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<InvestmentInstrument> InvestmentInstruments { get; set; }
        public DbSet<FundamentalData> Fundamentals { get; set; }
        public DbSet<PriceData> PriceDataSet { get; set; }
        public DbSet<Recommendation> Recommendations { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Aplikace konfigurací z podsložky Configurations 
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
