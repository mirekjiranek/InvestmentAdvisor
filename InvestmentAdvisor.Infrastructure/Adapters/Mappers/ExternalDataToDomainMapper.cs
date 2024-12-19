using Domain.Entities;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Adapters.Mappers
{
    /// <summary>
    /// Mapper pro převod dat z externích API do doménových entit.
    /// </summary>
    public static class ExternalDataToDomainMapper
    {
        /// <summary>
        /// Mapuje cenová data z JSON na doménovou entitu PriceData.
        /// </summary>
        public static PriceData MapPriceData(JsonElement dailyData, DateTime date)
        {
            return new PriceData(
                date: date,
                open: dailyData.GetProperty("1. open").GetDecimal(),
                high: dailyData.GetProperty("2. high").GetDecimal(),
                low: dailyData.GetProperty("3. low").GetDecimal(),
                close: dailyData.GetProperty("4. close").GetDecimal(),
                volume: dailyData.GetProperty("5. volume").GetInt64()
            );
        }

        /// <summary>
        /// Mapuje fundamentální data z Finnhub API na FundamentalData.
        /// </summary>
        public static FundamentalData MapFundamentalData(JsonElement companyProfile)
        {
            var valuation = new ValuationMetrics(
                PE: companyProfile.GetProperty("peRatio").GetDecimal(),
                PB: companyProfile.GetProperty("pbRatio").GetDecimal(),
                EV_EBITDA: companyProfile.GetProperty("evToEbitda").GetDecimal(),
                EV_EBIT: 0m, // Přidat další logiku, pokud je dostupné
                PriceSales: companyProfile.GetProperty("priceToSales").GetDecimal(),
                PriceCashFlow: 0m
            );

            var growth = new GrowthMetrics(
                HistoricalEpsGrowth: companyProfile.GetProperty("epsGrowth5Y").GetDecimal(),
                PredictedEpsGrowth: companyProfile.GetProperty("epsGrowth1Y").GetDecimal(),
                RevenueGrowth: companyProfile.GetProperty("revenueGrowth5Y").GetDecimal(),
                ProfitGrowth: 0m,
                DividendGrowth: 0m,
                PredictedFCFGrowth: 0.05m, // Předpoklad nebo jiná logika
                LongTermGrowthRate: 0.03m
            );

            var profitability = new ProfitabilityMetrics(
                ROE: companyProfile.GetProperty("returnOnEquity").GetDecimal(),
                ROA: companyProfile.GetProperty("returnOnAssets").GetDecimal(),
                GrossMargin: companyProfile.GetProperty("grossMargin").GetDecimal(),
                OperatingMargin: companyProfile.GetProperty("operatingMargin").GetDecimal(),
                NetMargin: companyProfile.GetProperty("netProfitMargin").GetDecimal()
            );

            var stability = new StabilityMetrics(
                DebtToEquity: companyProfile.GetProperty("debtToEquity").GetDecimal(),
                CurrentRatio: companyProfile.GetProperty("currentRatio").GetDecimal(),
                QuickRatio: 0m, // Přidat logiku, pokud dostupné
                InterestCoverage: companyProfile.GetProperty("interestCoverage").GetDecimal()
            );

            var dividend = new DividendMetrics(
                DividendYield: companyProfile.GetProperty("dividendYield").GetDecimal(),
                DividendPayoutRatio: companyProfile.GetProperty("payoutRatio").GetDecimal(),
                DividendGrowth: 0.02m,
                CurrentAnnualDividend: companyProfile.GetProperty("annualDividend").GetDecimal()
            );

            var marketRisk = new MarketRiskMetrics(
                Beta: companyProfile.GetProperty("beta").GetDecimal(),
                SharpeRatio: 0m, // Lze spočítat zvlášť
                StandardDeviation: 0m
            );

            var sentiment = new SentimentMetrics(
                ConsensusTargetPrice: companyProfile.GetProperty("targetPrice").GetDecimal(),
                AnalystRecommendation: companyProfile.GetProperty("analystRecommendation").GetString(),
                MediaSentimentScore: 0.8m // Defaultní hodnota
            );

            var comparable = new ComparableMetrics(
                SectorAveragePE: companyProfile.GetProperty("sectorPE").GetDecimal(),
                SectorMedianPB: companyProfile.GetProperty("sectorPB").GetDecimal(),
                PeerEVEBITDA: companyProfile.GetProperty("sectorEVEBITDA").GetDecimal(),
                SectorPriceSales: companyProfile.GetProperty("sectorPriceSales").GetDecimal()
            );

            var earnings = new EarningsMetrics(
                EPS: companyProfile.GetProperty("eps").GetDecimal(),
                EBITDA: companyProfile.GetProperty("ebitda").GetDecimal()
            );

            var revenue = new RevenueMetrics(
                SalesPerShare: companyProfile.GetProperty("salesPerShare").GetDecimal()
            );

            var cashFlow = new CashFlowMetrics(
                CurrentFCF: companyProfile.GetProperty("freeCashFlow").GetDecimal(),
                ProjectedFCFGrowth: 0.05m
            );

            var costOfCapital = new CostOfCapitalMetrics(
                WACC: 0.08m, // Defaultní nebo externě vypočtená hodnota
                RequiredReturnOnEquity: 0.10m
            );

            return new FundamentalData(
                valuation: valuation,
                growth: growth,
                profitability: profitability,
                stability: stability,
                dividend: dividend,
                marketRisk: marketRisk,
                sentiment: sentiment,
                comparable: comparable,
                earnings: earnings,
                revenue: revenue,
                cashFlow: cashFlow,
                costOfCapital: costOfCapital
            );
        }

        /// <summary>
        /// Aktualizuje sentiment a odhady analytiků pro existující FundamentalData.
        /// </summary>
        public static FundamentalData MapSentimentAndEstimates(FundamentalData data, JsonElement sentimentData)
        {
            var updatedSentiment = new SentimentMetrics(
                ConsensusTargetPrice: sentimentData.GetProperty("targetPrice").GetDecimal(),
                AnalystRecommendation: sentimentData.GetProperty("recommendation").GetString(),
                MediaSentimentScore: sentimentData.GetProperty("mediaSentiment").GetDecimal()
            );

            return new FundamentalData(
                valuation: data.Valuation,
                growth: data.Growth,
                profitability: data.Profitability,
                stability: data.Stability,
                dividend: data.Dividend,
                marketRisk: data.MarketRisk,
                sentiment: updatedSentiment,
                comparable: data.Comparable,
                earnings: data.Earnings,
                revenue: data.Revenue,
                cashFlow: data.CashFlow,
                costOfCapital: data.CostOfCapital
            );
        }
    }
}
