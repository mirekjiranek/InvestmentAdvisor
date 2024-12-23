using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class PriceDataConfiguration : IEntityTypeConfiguration<PriceData>
    {
        public void Configure(EntityTypeBuilder<PriceData> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Open)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(p => p.High)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(p => p.Low)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(p => p.Close)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(p => p.Volume)
                   .HasColumnType("bigint")
                   .IsRequired();

            builder.Property(p => p.Date)
                   .HasColumnType("date")
                   .IsRequired();
        }
    }
}
