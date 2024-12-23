using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class InvestmentInstrumentConfiguration : IEntityTypeConfiguration<InvestmentInstrument>
    {
        public void Configure(EntityTypeBuilder<InvestmentInstrument> builder)
        {
            builder.HasKey(i => i.InvestmentInstrumentId);
            builder.Property(i => i.Symbol).IsRequired().HasMaxLength(50);
            builder.Property(i => i.Name).HasMaxLength(200);

            // Jedna investice má 1 FundamentalData
            builder.HasOne(i => i.FundamentalData)
                   .WithOne()
                   .HasForeignKey<FundamentalData>(f => f.InvestmentInstrumentId);

            // Jedna investice má N PriceData
            builder.HasMany(i => i.PriceHistory)
                   .WithOne()
                   .HasForeignKey(p => p.InvestmentInstrumentId);

            // Jedna investice má 1 Recommendation
            builder.HasOne(i => i.CurrentRecommendation)
                   .WithOne()
                   .HasForeignKey<Recommendation>(r => r.InvestmentInstrumentId);
        }
    }
}
