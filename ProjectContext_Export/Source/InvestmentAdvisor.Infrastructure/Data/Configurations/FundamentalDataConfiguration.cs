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
    public class FundamentalDataConfiguration : IEntityTypeConfiguration<FundamentalData>
    {
        public void Configure(EntityTypeBuilder<FundamentalData> builder)
        {
            builder.HasKey(f => f.Id);

            // Vztah: jedna FundamentalData náleží jednomu InvestmentInstrument
            builder.HasOne<InvestmentInstrument>()
                   .WithOne(i => i.FundamentalData)
                   .HasForeignKey<FundamentalData>(f => f.InvestmentInstrumentId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Jelikož ValuationMetrics, GrowthMetrics atd. jsou ValueObjects,
            // uložíme je pomocí OwnsOne:

            builder.OwnsOne(f => f.Valuation, vo =>
            {
                vo.Property(v => v.PE).HasColumnName("Valuation_PE");
                vo.Property(v => v.PB).HasColumnName("Valuation_PB");
                vo.Property(v => v.EV_EBITDA).HasColumnName("Valuation_EVEBITDA");
                vo.Property(v => v.EV_EBIT).HasColumnName("Valuation_EVEBIT");
                vo.Property(v => v.PriceSales).HasColumnName("Valuation_PriceSales");
                vo.Property(v => v.PriceCashFlow).HasColumnName("Valuation_PriceCashFlow");
            });

            builder.OwnsOne(f => f.Growth, go =>
            {
                go.Property(g => g.HistoricalEpsGrowth).HasColumnName("Growth_HistoricalEps");
                go.Property(g => g.PredictedEpsGrowth).HasColumnName("Growth_PredictedEps");
                go.Property(g => g.RevenueGrowth).HasColumnName("Growth_Revenue");
                go.Property(g => g.ProfitGrowth).HasColumnName("Growth_Profit");
                go.Property(g => g.DividendGrowth).HasColumnName("Growth_Dividend");
                go.Property(g => g.PredictedFCFGrowth).HasColumnName("Growth_PredictedFCF");
                go.Property(g => g.LongTermGrowthRate).HasColumnName("Growth_LongTerm");
            });

            builder.OwnsOne(f => f.Profitability, po =>
            {
                po.Property(p => p.ROE).HasColumnName("Profitability_ROE");
                po.Property(p => p.ROA).HasColumnName("Profitability_ROA");
                po.Property(p => p.GrossMargin).HasColumnName("Profitability_GrossMargin");
                po.Property(p => p.OperatingMargin).HasColumnName("Profitability_OperatingMargin");
                po.Property(p => p.NetMargin).HasColumnName("Profitability_NetMargin");
            });

            builder.OwnsOne(f => f.Stability, so =>
            {
                so.Property(s => s.DebtToEquity).HasColumnName("Stability_DebtToEquity");
                so.Property(s => s.CurrentRatio).HasColumnName("Stability_CurrentRatio");
                so.Property(s => s.QuickRatio).HasColumnName("Stability_QuickRatio");
                so.Property(s => s.InterestCoverage).HasColumnName("Stability_InterestCoverage");
            });

            builder.OwnsOne(f => f.Dividend, doo =>
            {
                doo.Property(d => d.DividendYield).HasColumnName("Dividend_Yield");
                doo.Property(d => d.DividendPayoutRatio).HasColumnName("Dividend_PayoutRatio");
                doo.Property(d => d.DividendGrowth).HasColumnName("Dividend_Growth");
                doo.Property(d => d.CurrentAnnualDividend).HasColumnName("Dividend_CurrentAnnual");
            });

            builder.OwnsOne(f => f.MarketRisk, mo =>
            {
                mo.Property(m => m.Beta).HasColumnName("MarketRisk_Beta");
                mo.Property(m => m.SharpeRatio).HasColumnName("MarketRisk_SharpeRatio");
                mo.Property(m => m.StandardDeviation).HasColumnName("MarketRisk_StdDev");
            });

            builder.OwnsOne(f => f.Sentiment, se =>
            {
                se.Property(s => s.ConsensusTargetPrice).HasColumnName("Sentiment_TargetPrice");
                se.Property(s => s.AnalystRecommendation).HasColumnName("Sentiment_AnalystRec");
                se.Property(s => s.MediaSentimentScore).HasColumnName("Sentiment_MediaScore");
            });

            builder.OwnsOne(f => f.Comparable, co =>
            {
                co.Property(c => c.SectorAveragePE).HasColumnName("Comp_SectorAvgPE");
                co.Property(c => c.SectorMedianPB).HasColumnName("Comp_SectorMedPB");
                co.Property(c => c.PeerEVEBITDA).HasColumnName("Comp_PeerEVEBITDA");
                co.Property(c => c.SectorPriceSales).HasColumnName("Comp_SectorPriceSales");
            });

            builder.OwnsOne(f => f.Earnings, eo =>
            {
                eo.Property(e => e.EPS).HasColumnName("Earnings_EPS");
                eo.Property(e => e.EBITDA).HasColumnName("Earnings_EBITDA");
            });

            builder.OwnsOne(f => f.Revenue, ro =>
            {
                ro.Property(r => r.SalesPerShare).HasColumnName("Revenue_SalesPerShare");
            });

            builder.OwnsOne(f => f.CashFlow, cfo =>
            {
                cfo.Property(c => c.CurrentFCF).HasColumnName("CashFlow_CurrentFCF");
                cfo.Property(c => c.ProjectedFCFGrowth).HasColumnName("CashFlow_ProjectedFCFGrowth");
            });

            builder.OwnsOne(f => f.CostOfCapital, w =>
            {
                w.Property(wc => wc.WACC).HasColumnName("CostOfCapital_WACC");
                w.Property(wc => wc.RequiredReturnOnEquity).HasColumnName("CostOfCapital_RROE");
            });
        }
    }
}
