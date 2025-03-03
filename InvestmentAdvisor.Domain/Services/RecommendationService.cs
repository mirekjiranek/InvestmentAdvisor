using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;

namespace Domain.Services
{
    /// <summary>
    /// Vylepšený servis pro generování investičních doporučení:
    /// 1. Nabízí doporučení s různými časovými horizonty (krátkodobé, střednědobé, dlouhodobé)
    /// 2. Zohledňuje momentum a technické indikátory
    /// 3. Implementuje dynamické prahové hodnoty pro doporučení podle volatility sektoru
    /// 4. Přidává odvětvový kontext k doporučení (relativní postavení v sektoru)
    /// 5. Generuje detailní zdůvodnění s klíčovými faktory pro rozhodnutí
    /// </summary>
    public class RecommendationService : IRecommendationService
    {
        private readonly IValuationService _valuationService;

        public RecommendationService(IValuationService valuationService)
        {
            _valuationService = valuationService;
        }

        public Recommendation GenerateRecommendation(InvestmentInstrument instrument)
        {
            if (instrument.FundamentalData == null || instrument.PriceHistory.Count == 0)
            {
                throw new InvalidOperationException("Instrument requires fundamental and price data.");
            }

            // Vypočítáme vnitřní hodnotu
            var intrinsicValue = _valuationService.CalculateIntrinsicValue(instrument);
            
            // Získáme aktuální tržní cenu
            var lastPrice = instrument.PriceHistory.OrderByDescending(p => p.Date).First().Close;

            // Aktualizujeme cenu ve fundamentálních datech pro výpočty
            var data = instrument.FundamentalData;
            data.Price = lastPrice;
            
            // Určení sektoru a typu společnosti
            string sector = DetermineSector(data);
            string companyType = DetermineCompanyType(data);
            
            // Výpočet různých metrik pro doporučení
            
            // 1. Základní potenciál růstu/poklesu
            decimal diffPercent = ((intrinsicValue - lastPrice) / lastPrice) * 100m;
            
            // 2. Momentum indikátory
            decimal momentumScore = CalculateMomentumScore(instrument);
            
            // 3. Technické indikátory
            decimal technicalScore = CalculateTechnicalScore(instrument);
            
            // 4. Kvalitativní faktory
            var qualityFactors = EvaluateQualityFactors(data);
            
            // 5. Valuační multiples vs sektor
            decimal valuationGrade = EvaluateValuationVsSector(data);
            
            // 6. Vyhodnocení rizika
            var riskAssessment = EvaluateRisk(data);
            
            // 7. Relativní postavení v sektoru
            var sectorPositioning = EvaluateSectorPositioning(data);
            
            // Dynamické prahové hodnoty podle volatility sektoru
            var thresholds = GetDynamicThresholds(sector, data.MarketRisk.Volatility);
            
            // Generování doporučení pro různé časové horizonty
            var shortTermRec = GenerateTimeframeRecommendation(
                intrinsicValue, lastPrice, momentumScore, technicalScore, 
                qualityFactors, valuationGrade, riskAssessment, thresholds, 
                "short", sector, companyType);
                
            var midTermRec = GenerateTimeframeRecommendation(
                intrinsicValue, lastPrice, momentumScore * 0.5m, technicalScore * 0.3m, 
                qualityFactors, valuationGrade * 1.2m, riskAssessment, thresholds, 
                "mid", sector, companyType);
                
            var longTermRec = GenerateTimeframeRecommendation(
                intrinsicValue, lastPrice, momentumScore * 0.2m, technicalScore * 0.1m, 
                qualityFactors, valuationGrade * 1.5m, riskAssessment, thresholds, 
                "long", sector, companyType);
            
            // Určení hlavního doporučení podle typu společnosti
            // Pro určité typy společností dáváme větší váhu dlouhodobému doporučení,
            // pro jiné krátkodobému, atd.
            RecommendationAction primaryAction;
            decimal primaryScore;
            string primaryHorizon;
            decimal primaryTargetPrice;
            
            switch (companyType)
            {
                case "Growth":
                case "Quality":
                case "Value":
                    // Pro dlouhodobější investice klademe důraz na střednědobé a dlouhodobé doporučení
                    primaryAction = longTermRec.action;
                    primaryScore = longTermRec.score;
                    primaryHorizon = "Long-term (2-3 years)";
                    primaryTargetPrice = longTermRec.targetPrice;
                    break;
                    
                case "Cyclical":
                case "Momentum":
                case "Turnaround":
                    // Pro cyklické a momentum tituly zdůrazňujeme střednědobý horizont
                    primaryAction = midTermRec.action;
                    primaryScore = midTermRec.score;
                    primaryHorizon = "Medium-term (6-12 months)";
                    primaryTargetPrice = midTermRec.targetPrice;
                    break;
                    
                case "Speculative":
                case "Event-driven":
                    // Pro spekulativní a event-driven případy zdůrazňujeme krátkodobější pohled
                    primaryAction = shortTermRec.action;
                    primaryScore = shortTermRec.score;
                    primaryHorizon = "Short-term (1-3 months)";
                    primaryTargetPrice = shortTermRec.targetPrice;
                    break;
                    
                default:
                    // Defaultně používáme střednědobý
                    primaryAction = midTermRec.action;
                    primaryScore = midTermRec.score;
                    primaryHorizon = "Medium-term (6-12 months)";
                    primaryTargetPrice = midTermRec.targetPrice;
                    break;
            }
            
            // Generování detailního zdůvodnění
            string rationale = GenerateRationale(
                intrinsicValue, lastPrice, diffPercent,
                momentumScore, technicalScore, qualityFactors,
                valuationGrade, riskAssessment, sectorPositioning,
                shortTermRec, midTermRec, longTermRec,
                sector, companyType);
            
            // Vytvoření finálního doporučení
            return new Recommendation(
                action: primaryAction,
                score: (int)primaryScore,
                timeHorizon: primaryHorizon,
                targetPrice: primaryTargetPrice,
                rationale: rationale,
                riskLevel: riskAssessment.riskLevel,
                sectorPosition: sectorPositioning.position,
                shortTermOutlook: shortTermRec.action.ToString(),
                midTermOutlook: midTermRec.action.ToString(),
                longTermOutlook: longTermRec.action.ToString()
            );
        }
        
        /// <summary>
        /// Určí sektor, do kterého společnost patří
        /// </summary>
        private string DetermineSector(FundamentalData data)
        {
            // V reálné implementaci by toto bylo určeno z metadat společnosti
            // Pro demonstraci používáme zjednodušené určení na základě charakteristik
            
            if (data.Profitability.NetMargin > 0.2m && data.Stability.DebtToEquity < 0.5m)
                return "Technology";
            else if (data.Profitability.NetMargin > 0.15m && data.Dividend.DividendYield > 0.03m)
                return "Consumer";
            else if (data.Stability.DebtToEquity > 1.5m && data.Dividend.DividendYield > 0.04m)
                return "Utilities";
            else if (data.Stability.DebtToEquity > 1.2m && data.Profitability.NetMargin < 0.08m)
                return "Industrial";
            else if (data.Profitability.NetMargin > 0.25m && data.Stability.CurrentRatio > 2.0m)
                return "Healthcare";
            else if (data.Stability.DebtToEquity > 2.0m)
                return "Financial";
            else
                return "Other";
        }
        
        /// <summary>
        /// Určí typ společnosti na základě jejích charakteristik
        /// </summary>
        private string DetermineCompanyType(FundamentalData data)
        {
            // Zjednodušené určení typu společnosti
            
            if (data.Growth.RevenueGrowth > 0.25m && data.Growth.EarningsGrowth > 0.3m)
                return "Growth";
            else if (data.Profitability.ROE > 0.18m && data.Stability.DebtToEquity < 0.8m)
                return "Quality";
            else if (data.Valuation.PE < 12.0m && data.Valuation.PB < 1.2m)
                return "Value";
            else if (data.MarketRisk.Beta > 1.3m && data.MarketRisk.Volatility > 0.3m)
                return "Cyclical";
            else if (data.Growth.RevenueGrowth > 0.15m && data.Earnings.EPS <= 0)
                return "Speculative";
            else if (data.Dividend.DividendYield > 0.04m && data.Stability.DebtToEquity < 1.2m)
                return "Income";
            else if (data.MarketRisk.Beta > 1.2m && data.Sentiment.AnalystConsensus > 4.0m)
                return "Momentum";
            else if (data.Earnings.EPS < 0 && data.Sentiment.AnalystConsensus > 3.5m)
                return "Turnaround";
            else
                return "Balanced";
        }
        
        /// <summary>
        /// Vypočítá skóre momentu (trendu) na základě cenové historie
        /// </summary>
        private decimal CalculateMomentumScore(InvestmentInstrument instrument)
        {
            // Momentum je založeno na:
            // 1. Krátkodobém cenovém trendu (1 měsíc)
            // 2. Střednědobém cenovém trendu (3 měsíce)
            // 3. Objemu obchodů
            
            var priceHistory = instrument.PriceHistory.OrderByDescending(p => p.Date).ToList();
            if (priceHistory.Count < 90) // potřebujeme alespoň 3 měsíce dat
                return 0;
                
            var currentPrice = priceHistory[0].Close;
            
            // 1-měsíční trend (cca 21 obchodních dní)
            var oneMoReturn = 0m;
            if (priceHistory.Count >= 21)
                oneMoReturn = (currentPrice / priceHistory[21].Close - 1) * 100;
                
            // 3-měsíční trend (cca 63 obchodních dní)
            var threeMoReturn = 0m;
            if (priceHistory.Count >= 63)
                threeMoReturn = (currentPrice / priceHistory[63].Close - 1) * 100;
                
            // Váhy pro jednotlivé složky
            decimal oneMoWeight = 0.6m;
            decimal threeMoWeight = 0.4m;
            
            // Normalizace na skóre 0-10
            decimal oneMoScore = NormalizeMomentumScore(oneMoReturn);
            decimal threeMoScore = NormalizeMomentumScore(threeMoReturn);
            
            // Vážené skóre
            decimal momentumScore = (oneMoScore * oneMoWeight) + (threeMoScore * threeMoWeight);
            
            return momentumScore;
        }
        
        /// <summary>
        /// Normalizuje procentuální návratnost na skóre 0-10
        /// </summary>
        private decimal NormalizeMomentumScore(decimal percentReturn)
        {
            // -20% nebo horší = 0
            // -10% = 2
            // 0% = 5
            // +10% = 8
            // +20% nebo lepší = 10
            
            if (percentReturn <= -20)
                return 0;
            else if (percentReturn <= -10)
                return 2 + ((percentReturn + 20) / 10) * 2;
            else if (percentReturn <= 0)
                return 4 + ((percentReturn + 10) / 10);
            else if (percentReturn <= 10)
                return 5 + (percentReturn / 10) * 3;
            else if (percentReturn <= 20)
                return 8 + ((percentReturn - 10) / 10) * 2;
            else
                return 10;
        }
        
        /// <summary>
        /// Vypočítá skóre technických indikátorů
        /// </summary>
        private decimal CalculateTechnicalScore(InvestmentInstrument instrument)
        {
            // V této demonstrační verzi vrátíme neutrální skóre
            // V reálné implementaci by zde byly výpočty RSI, MACD, klouzavých průměrů, atd.
            return 5.0m;
        }
        
        /// <summary>
        /// Vyhodnotí kvalitativní faktory společnosti
        /// </summary>
        private (bool isHighQuality, bool hasPositiveMomentum, string[] strengths, string[] weaknesses)
            EvaluateQualityFactors(FundamentalData data)
        {
            // Vyhodnocujeme silné a slabé stránky společnosti
            var strengths = new List<string>();
            var weaknesses = new List<string>();
            
            // Ziskovost
            bool isHighProfitability = data.Profitability.ROE > 0.15m && data.Profitability.NetMargin > 0.1m;
            if (isHighProfitability)
                strengths.Add("High profitability");
            else if (data.Profitability.ROE < 0.05m || data.Profitability.NetMargin < 0.03m)
                weaknesses.Add("Low profitability");
                
            // Růst
            bool hasStrongGrowth = data.Growth.RevenueGrowth > 0.1m && data.Growth.EarningsGrowth > 0.12m;
            if (hasStrongGrowth)
                strengths.Add("Strong growth");
            else if (data.Growth.RevenueGrowth < 0.03m || data.Growth.EarningsGrowth < 0.0m)
                weaknesses.Add("Weak growth");
                
            // Finanční zdraví
            bool isSolid = data.Stability.DebtToEquity < 1.0m && data.Stability.InterestCoverage > 5.0m;
            if (isSolid)
                strengths.Add("Solid financial health");
            else if (data.Stability.DebtToEquity > 2.0m || data.Stability.InterestCoverage < 2.0m)
                weaknesses.Add("Financial weakness");
                
            // Dividenda
            if (data.Dividend.DividendYield > 0.03m && data.Dividend.PayoutRatio < 0.7m)
                strengths.Add("Attractive dividend");
            else if (data.Dividend.DividendYield > 0.06m && data.Dividend.PayoutRatio > 0.8m)
                weaknesses.Add("Potentially unsustainable dividend");
                
            // Sentiment
            if (data.Sentiment.AnalystConsensus > 4.0m)
                strengths.Add("Positive analyst sentiment");
            else if (data.Sentiment.AnalystConsensus < 2.5m)
                weaknesses.Add("Negative analyst sentiment");
                
            // Momentum
            bool hasPositiveMomentum = data.Growth.PredictedEpsGrowth > 0.10m && data.Growth.SalesGrowth > 0.05m;
            
            // Celkové vyhodnocení kvality
            bool isHighQuality = isHighProfitability && isSolid && strengths.Count > weaknesses.Count;
            
            return (isHighQuality, hasPositiveMomentum, strengths.ToArray(), weaknesses.ToArray());
        }
        
        /// <summary>
        /// Vyhodnotí valuační metriky společnosti v porovnání se sektorem
        /// </summary>
        private decimal EvaluateValuationVsSector(FundamentalData data)
        {
            // Porovnáváme hlavní valuační násobky s průměrem sektoru
            // Skóre 0-10, kde vyšší hodnota = lepší valuace (podhodnocení)
            
            decimal valuationScore = 0;
            int factorsCount = 0;
            
            // P/E ratio
            if (data.Valuation.PE > 0 && data.Comparable.SectorAveragePE > 0)
            {
                decimal peRatio = data.Valuation.PE / data.Comparable.SectorAveragePE;
                
                if (peRatio < 0.6m) valuationScore += 9;
                else if (peRatio < 0.8m) valuationScore += 7;
                else if (peRatio < 1.0m) valuationScore += 6;
                else if (peRatio < 1.2m) valuationScore += 4;
                else if (peRatio < 1.5m) valuationScore += 2;
                else valuationScore += 1;
                
                factorsCount++;
            }
            
            // EV/EBITDA
            if (data.Valuation.EVEBITDA > 0 && data.Comparable.PeerEVEBITDA > 0)
            {
                decimal evEbitdaRatio = data.Valuation.EVEBITDA / data.Comparable.PeerEVEBITDA;
                
                if (evEbitdaRatio < 0.6m) valuationScore += 9;
                else if (evEbitdaRatio < 0.8m) valuationScore += 7;
                else if (evEbitdaRatio < 1.0m) valuationScore += 6;
                else if (evEbitdaRatio < 1.2m) valuationScore += 4;
                else if (evEbitdaRatio < 1.5m) valuationScore += 2;
                else valuationScore += 1;
                
                factorsCount++;
            }
            
            // P/S
            if (data.Valuation.PS > 0 && data.Comparable.SectorPriceSales > 0)
            {
                decimal psRatio = data.Valuation.PS / data.Comparable.SectorPriceSales;
                
                if (psRatio < 0.6m) valuationScore += 9;
                else if (psRatio < 0.8m) valuationScore += 7;
                else if (psRatio < 1.0m) valuationScore += 6;
                else if (psRatio < 1.2m) valuationScore += 4;
                else if (psRatio < 1.5m) valuationScore += 2;
                else valuationScore += 1;
                
                factorsCount++;
            }
            
            // P/B
            if (data.Valuation.PB > 0 && data.Comparable.SectorPriceBook > 0)
            {
                decimal pbRatio = data.Valuation.PB / data.Comparable.SectorPriceBook;
                
                if (pbRatio < 0.6m) valuationScore += 9;
                else if (pbRatio < 0.8m) valuationScore += 7;
                else if (pbRatio < 1.0m) valuationScore += 6;
                else if (pbRatio < 1.2m) valuationScore += 4;
                else if (pbRatio < 1.5m) valuationScore += 2;
                else valuationScore += 1;
                
                factorsCount++;
            }
            
            // Vrátíme průměrné skóre
            if (factorsCount > 0)
                return valuationScore / factorsCount;
            else
                return 5.0m; // Neutrální hodnota
        }
        
        /// <summary>
        /// Vyhodnocuje rizikový profil společnosti
        /// </summary>
        private (string riskLevel, decimal riskScore, string[] riskFactors)
            EvaluateRisk(FundamentalData data)
        {
            // Rizikové faktory
            var riskFactors = new List<string>();
            
            decimal beta = data.MarketRisk.Beta;
            decimal volatility = data.MarketRisk.Volatility;
            decimal debtToEquity = data.Stability.DebtToEquity;
            decimal currentRatio = data.Stability.CurrentRatio;
            decimal interestCoverage = data.Stability.InterestCoverage;
            
            // Beta
            if (beta > 1.5m)
                riskFactors.Add($"High beta ({beta:F2})");
                
            // Volatilita
            if (volatility > 0.3m)
                riskFactors.Add($"High volatility ({volatility * 100:F1}%)");
                
            // Zadlužení
            if (debtToEquity > 2.0m)
                riskFactors.Add($"High debt (D/E: {debtToEquity:F2}x)");
                
            // Likvidita
            if (currentRatio < 1.0m)
                riskFactors.Add($"Liquidity concerns (CR: {currentRatio:F2}x)");
                
            // Úrokové krytí
            if (interestCoverage < 3.0m)
                riskFactors.Add($"Low interest coverage ({interestCoverage:F2}x)");
            
            // Velikost
            if (data.Revenue.SalesPerShare * data.Price < 500m) // Hrubý odhad tržní kapitalizace
                riskFactors.Add("Small capitalization");
                
            // Výpočet rizikového skóre (vyšší = více rizikové)
            decimal riskScore = 0;
            
            // Beta faktor (0-3 body)
            riskScore += beta < 0.8m ? 0 : beta < 1.2m ? 1 : beta < 1.6m ? 2 : 3;
            
            // Volatilita (0-3 body)
            riskScore += volatility < 0.2m ? 0 : volatility < 0.3m ? 1 : volatility < 0.4m ? 2 : 3;
            
            // Zadlužení (0-3 body)
            riskScore += debtToEquity < 1.0m ? 0 : debtToEquity < 1.5m ? 1 : debtToEquity < 2.5m ? 2 : 3;
            
            // Likvidita (0-2 body)
            riskScore += currentRatio > 1.5m ? 0 : currentRatio > 1.0m ? 1 : 2;
            
            // Úrokové krytí (0-2 body)
            riskScore += interestCoverage > 5.0m ? 0 : interestCoverage > 2.0m ? 1 : 2;
            
            // Určení rizikové úrovně
            string riskLevel;
            
            if (riskScore <= 2)
                riskLevel = "Very Low";
            else if (riskScore <= 4)
                riskLevel = "Low";
            else if (riskScore <= 6)
                riskLevel = "Moderate";
            else if (riskScore <= 8)
                riskLevel = "High";
            else
                riskLevel = "Very High";
                
            return (riskLevel, riskScore, riskFactors.ToArray());
        }
        
        /// <summary>
        /// Vyhodnocuje relativní postavení společnosti v jejím sektoru
        /// </summary>
        private (string position, string description)
            EvaluateSectorPositioning(FundamentalData data)
        {
            // Hodnotíme postavení v sektoru na základě několika faktorů:
            // 1. Ziskovost vs. průměr sektoru
            // 2. Růst vs. průměr sektoru
            // 3. Valuace vs. průměr sektoru
            
            // Ziskovost
            decimal profitabilityRatio = 1.0m;
            if (data.Comparable.SectorAverageROE > 0 && data.Profitability.ROE > 0)
                profitabilityRatio = data.Profitability.ROE / data.Comparable.SectorAverageROE;
                
            // Marže
            decimal marginRatio = 1.0m;
            if (data.Comparable.SectorAverageNetMargin > 0 && data.Profitability.NetMargin > 0)
                marginRatio = data.Profitability.NetMargin / data.Comparable.SectorAverageNetMargin;
            
            // Růst
            decimal growthRatio = 1.0m;
            if (data.Comparable.SectorAverageGrowth > 0 && data.Growth.EarningsGrowth > 0)
                growthRatio = data.Growth.EarningsGrowth / data.Comparable.SectorAverageGrowth;
                
            // Průměrný relativní výkon
            decimal overallRatio = (profitabilityRatio + marginRatio + growthRatio) / 3;
            
            // Určení pozice
            string position;
            string description;
            
            if (overallRatio > 1.5m)
            {
                position = "Leader";
                description = "Significantly outperforms sector averages in profitability and growth";
            }
            else if (overallRatio > 1.2m)
            {
                position = "Strong performer";
                description = "Above average performance compared to sector peers";
            }
            else if (overallRatio > 0.8m)
            {
                position = "Average";
                description = "Performance in line with sector averages";
            }
            else if (overallRatio > 0.5m)
            {
                position = "Below average";
                description = "Underperforms sector averages in key metrics";
            }
            else
            {
                position = "Laggard";
                description = "Significantly underperforms sector averages";
            }
            
            return (position, description);
        }
        
        /// <summary>
        /// Poskytuje dynamické prahové hodnoty pro doporučení podle volatility sektoru
        /// </summary>
        private (decimal strongBuy, decimal buy, decimal accumulate, decimal reduce, decimal sell)
            GetDynamicThresholds(string sector, decimal volatility)
        {
            // Základní prahové hodnoty (procentuální potenciál/pokles pro různá doporučení)
            decimal strongBuyThreshold = 30m;    // > 30% potenciál
            decimal buyThreshold = 15m;          // > 15% potenciál
            decimal accumulateThreshold = 5m;    // > 5% potenciál
            decimal reduceThreshold = -5m;       // < -5% potenciál (pokles)
            decimal sellThreshold = -15m;        // < -15% potenciál (pokles)
            
            // Úprava podle volatility - vyšší volatilita = vyšší prahové hodnoty
            decimal volatilityAdjustment = (volatility - 0.2m) * 100m; // 0.2 = 20% volatilita jako referenční hodnota
            
            if (volatilityAdjustment > 0)
            {
                strongBuyThreshold += volatilityAdjustment * 0.5m;
                buyThreshold += volatilityAdjustment * 0.3m;
                accumulateThreshold += volatilityAdjustment * 0.2m;
                reduceThreshold -= volatilityAdjustment * 0.2m;
                sellThreshold -= volatilityAdjustment * 0.3m;
            }
            
            // Úprava podle sektoru
            switch (sector)
            {
                case "Technology": // Vyšší volatilita a růst = vyšší prahové hodnoty
                    strongBuyThreshold += 5m;
                    buyThreshold += 3m;
                    break;
                    
                case "Utilities": // Nižší volatilita = nižší prahové hodnoty
                    strongBuyThreshold -= 5m;
                    buyThreshold -= 3m;
                    accumulateThreshold -= 1m;
                    reduceThreshold += 1m;
                    sellThreshold += 3m;
                    break;
                    
                case "Financial": // Vyšší nároky na marži bezpečnosti
                    strongBuyThreshold += 5m;
                    buyThreshold += 2m;
                    break;
            }
            
            return (strongBuyThreshold, buyThreshold, accumulateThreshold, reduceThreshold, sellThreshold);
        }
        
        /// <summary>
        /// Generuje doporučení pro specifický časový horizont
        /// </summary>
        private (RecommendationAction action, decimal score, decimal targetPrice)
            GenerateTimeframeRecommendation(
                decimal intrinsicValue, decimal currentPrice, decimal momentumScore, decimal technicalScore,
                (bool isHighQuality, bool hasPositiveMomentum, string[] strengths, string[] weaknesses) qualityFactors,
                decimal valuationGrade, (string riskLevel, decimal riskScore, string[] riskFactors) riskAssessment,
                (decimal strongBuy, decimal buy, decimal accumulate, decimal reduce, decimal sell) thresholds,
                string timeframe, string sector, string companyType)
        {
            // Výpočet základního potenciálu
            decimal diffPercent = ((intrinsicValue - currentPrice) / currentPrice) * 100m;
            
            // Úprava podle časového horizontu
            decimal adjustedPotential = diffPercent;
            
            // Váhy pro různé faktory podle časového horizontu
            decimal momentumWeight = 0, technicalWeight = 0, valuationWeight = 0, qualityWeight = 0;
            
            switch (timeframe)
            {
                case "short": // Krátkodobý horizont (1-3 měsíce)
                    // Krátkodobě hraje větší roli momentum a technická analýza
                    momentumWeight = 0.4m;
                    technicalWeight = 0.3m;
                    valuationWeight = 0.2m;
                    qualityWeight = 0.1m;
                    break;
                    
                case "mid": // Střednědobý horizont (6-12 měsíců)
                    // Vyvážená kombinace všech faktorů
                    momentumWeight = 0.2m;
                    technicalWeight = 0.1m;
                    valuationWeight = 0.4m;
                    qualityWeight = 0.3m;
                    break;
                    
                case "long": // Dlouhodobý horizont (2-3 roky)
                    // Dlouhodobě hraje větší roli valuace a kvalita
                    momentumWeight = 0.05m;
                    technicalWeight = 0.05m;
                    valuationWeight = 0.5m;
                    qualityWeight = 0.4m;
                    break;
            }
            
            // Normalizace momentum a technical skóre na procentuální vliv (-20% až +20%)
            decimal momentumEffect = (momentumScore - 5.0m) / 5.0m * 20.0m;
            decimal technicalEffect = (technicalScore - 5.0m) / 5.0m * 20.0m;
            
            // Vliv valuace (0-10 skóre převedené na -10% až +30% efekt)
            decimal valuationEffect = (valuationGrade - 5.0m) / 5.0m * (valuationGrade > 5.0m ? 30.0m : 10.0m);
            
            // Vliv kvality společnosti (prémie 0-15%)
            decimal qualityEffect = qualityFactors.isHighQuality ? 15.0m : 
                                   (qualityFactors.strengths.Length - qualityFactors.weaknesses.Length) * 3.0m;
            
            // Kombinace všech efektů s vahami podle časového horizontu
            adjustedPotential = diffPercent +
                               (momentumEffect * momentumWeight) +
                               (technicalEffect * technicalWeight) +
                               (valuationEffect * valuationWeight) +
                               (qualityEffect * qualityWeight);
            
            // Výběr akce na základě upraveného potenciálu a dynamických prahových hodnot
            RecommendationAction action;
            decimal score;
            
            if (adjustedPotential > thresholds.strongBuy)
            {
                action = RecommendationAction.StrongBuy;
                score = 1;
            }
            else if (adjustedPotential > thresholds.buy)
            {
                action = RecommendationAction.Buy;
                score = 2;
            }
            else if (adjustedPotential > thresholds.accumulate)
            {
                action = RecommendationAction.Accumulate;
                score = 3;
            }
            else if (adjustedPotential > thresholds.reduce)
            {
                action = RecommendationAction.Hold;
                score = 4;
            }
            else if (adjustedPotential > thresholds.sell)
            {
                action = RecommendationAction.Reduce;
                score = 5;
            }
            else if (adjustedPotential > thresholds.sell * 1.5m)
            {
                action = RecommendationAction.Sell;
                score = 6;
            }
            else
            {
                action = RecommendationAction.StrongSell;
                score = 7;
            }
            
            // Úprava cílové ceny podle časového horizontu a rizika
            decimal targetPriceAdjustment = 1.0m;
            
            // Konzervativnější cílové ceny pro rizikovější instrumenty
            if (riskAssessment.riskLevel == "High" || riskAssessment.riskLevel == "Very High")
            {
                targetPriceAdjustment = 0.85m; // 15% konzervativnější
            }
            
            // Dlouhodobě vyšší očekávaný potenciál
            if (timeframe == "long" && action == RecommendationAction.Buy || action == RecommendationAction.StrongBuy)
            {
                targetPriceAdjustment = 1.1m; // 10% vyšší
            }
            
            // Výpočet cílové ceny
            decimal targetPrice = currentPrice + ((intrinsicValue - currentPrice) * targetPriceAdjustment);
            
            return (action, score, targetPrice);
        }
        
        /// <summary>
        /// Generuje detailní zdůvodnění doporučení
        /// </summary>
        private string GenerateRationale(
            decimal intrinsicValue, decimal currentPrice, decimal diffPercent,
            decimal momentumScore, decimal technicalScore,
            (bool isHighQuality, bool hasPositiveMomentum, string[] strengths, string[] weaknesses) qualityFactors,
            decimal valuationGrade, (string riskLevel, decimal riskScore, string[] riskFactors) riskAssessment,
            (string position, string description) sectorPositioning,
            (RecommendationAction action, decimal score, decimal targetPrice) shortTermRec,
            (RecommendationAction action, decimal score, decimal targetPrice) midTermRec,
            (RecommendationAction action, decimal score, decimal targetPrice) longTermRec,
            string sector, string companyType)
        {
            var sb = new StringBuilder();
            
            // Základní valuace
            sb.AppendLine($"Intrinsic value: {intrinsicValue:F2}, Current price: {currentPrice:F2}, Deviation: {diffPercent:F2}%");
            
            // Silné stránky
            if (qualityFactors.strengths.Length > 0)
            {
                sb.AppendLine("\nStrengths:");
                foreach (var strength in qualityFactors.strengths)
                {
                    sb.AppendLine($"- {strength}");
                }
            }
            
            // Slabé stránky
            if (qualityFactors.weaknesses.Length > 0)
            {
                sb.AppendLine("\nConcerns:");
                foreach (var weakness in qualityFactors.weaknesses)
                {
                    sb.AppendLine($"- {weakness}");
                }
            }
            
            // Rizikové faktory
            if (riskAssessment.riskFactors.Length > 0)
            {
                sb.AppendLine("\nRisk factors:");
                foreach (var riskFactor in riskAssessment.riskFactors)
                {
                    sb.AppendLine($"- {riskFactor}");
                }
            }
            
            // Odvětvový kontext
            sb.AppendLine($"\nSector positioning: {sectorPositioning.position} - {sectorPositioning.description}");
            
            // Časový horizont doporučení
            sb.AppendLine("\nRecommendation by timeframe:");
            sb.AppendLine($"- Short-term (1-3 months): {shortTermRec.action} (Target: {shortTermRec.targetPrice:F2})");
            sb.AppendLine($"- Medium-term (6-12 months): {midTermRec.action} (Target: {midTermRec.targetPrice:F2})");
            sb.AppendLine($"- Long-term (2-3 years): {longTermRec.action} (Target: {longTermRec.targetPrice:F2})");
            
            return sb.ToString();
        }
        
        public async Task<Recommendation> GenerateRecommendationAsync(InvestmentInstrument instrument, CancellationToken cancellationToken = default)
        {
            // Asynchronní implementace, která deleguje na synchronní metodu
            // V reálném scénáři by tato metoda mohla provádět další asynchronní operace
            return await Task.Run(() => GenerateRecommendation(instrument), cancellationToken);
        }
    }
}