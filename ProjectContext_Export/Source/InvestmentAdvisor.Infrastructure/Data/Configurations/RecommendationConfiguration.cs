using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data.Configurations
{
    public class RecommendationConfiguration : IEntityTypeConfiguration<Recommendation>
    {
        public void Configure(EntityTypeBuilder<Recommendation> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Action).HasConversion<string>().HasColumnName("Action").IsRequired();
            builder.Property(r => r.Score).HasColumnName("Score");
            builder.Property(r => r.TimeHorizon).HasColumnName("TimeHorizon");
            builder.Property(r => r.TargetPrice).HasColumnName("TargetPrice");
            builder.Property(r => r.Rationale).HasColumnName("Rationale");
            builder.Property(r => r.RiskLevel).HasColumnName("RiskLevel");
        }
    }
}
