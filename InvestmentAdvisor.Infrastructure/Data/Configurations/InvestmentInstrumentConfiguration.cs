using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class InvestmentInstrumentConfiguration : IEntityTypeConfiguration<InvestmentInstrument>
    {
        public void Configure(EntityTypeBuilder<InvestmentInstrument> builder)
        {
            // Definice primárního klíče
            builder.HasKey(i => i.InvestmentInstrumentId);

            // Definice vlastností s omezeními
            builder.Property(i => i.Symbol)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(i => i.Name)
                   .HasMaxLength(200);

            // Konfigurace jednoho k jednomu vztahu s FundamentalData
            builder.HasOne(i => i.FundamentalData)
                   .WithOne(f => f.InvestmentInstrument)
                   .HasForeignKey<FundamentalData>(f => f.InvestmentInstrumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Konfigurace jednoho k mnoha vztahu s PriceData
            builder.HasMany(i => i.PriceHistory)
                   .WithOne()
                   .HasForeignKey(p => p.InvestmentInstrumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Konfigurace jednoho k jednomu vztahu s Recommendation
            builder.HasOne(i => i.CurrentRecommendation)
                   .WithOne(r => r.InvestmentInstrument)
                   .HasForeignKey<Recommendation>(r => r.InvestmentInstrumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Další konfigurace (např. indexy) lze přidat zde, pokud je potřeba
        }
    }
}
