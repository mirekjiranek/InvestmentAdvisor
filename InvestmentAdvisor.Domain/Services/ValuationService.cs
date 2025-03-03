using Domain.Entities;
using Domain.Interfaces;
using Domain.Services.CalculationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class ValuationService : IValuationService
    {
        private readonly DCFModel _dcfModel;
        private readonly DDMModel _ddmModel;
        private readonly ComparableAnalysisModel _comparableModel;
        private readonly ScoreModel _scoreModel;

        public ValuationService()
        {
            _dcfModel = new DCFModel();
            _ddmModel = new DDMModel();
            _comparableModel = new ComparableAnalysisModel();
            _scoreModel = new ScoreModel();
        }

        public decimal CalculateIntrinsicValue(InvestmentInstrument instrument)
        {
            if (instrument.FundamentalData == null)
                throw new InvalidOperationException("Fundamental data is required.");

            var data = instrument.FundamentalData;
            
            // Získáme hodnoty z jednotlivých modelů
            decimal dcfValue = _dcfModel.CalculateDCFValue(data);
            decimal ddmValue = _ddmModel.CalculateDDMValue(data);
            decimal compValue = _comparableModel.CalculateComparableValue(data);
            decimal scoreValue = _scoreModel.CalculateScoreValue(data);
            
            // Ověříme, zda jsou hodnoty rozumné
            var lastPrice = instrument.PriceHistory.OrderByDescending(p => p.Date).FirstOrDefault()?.Close ?? 0;
            
            // Pokud některý z modelů dává extrémní hodnoty, omezíme jej
            if (lastPrice > 0)
            {
                // Omezíme hodnoty na max 5x aktuální ceny a min 0.2x aktuální ceny
                decimal maxAllowedValue = lastPrice * 5.0m;
                decimal minAllowedValue = lastPrice * 0.2m;
                
                dcfValue = Math.Max(minAllowedValue, Math.Min(maxAllowedValue, dcfValue));
                ddmValue = Math.Max(minAllowedValue, Math.Min(maxAllowedValue, ddmValue));
                compValue = Math.Max(minAllowedValue, Math.Min(maxAllowedValue, compValue));
                scoreValue = Math.Max(minAllowedValue, Math.Min(maxAllowedValue, scoreValue));
            }

            // Určení vah pro modely - dynamická adaptace podle dostupných dat a typu společnosti
            
            // Základní váhy
            decimal dcfWeight = 0.4m;
            decimal ddmWeight = 0.2m;
            decimal compWeight = 0.2m;
            decimal scoreWeight = 0.2m;
            
            // Adaptace vah podle typu společnosti
            
            // 1. Pokud není dividenda, DDM model nedává smysl 
            if (data.Dividend.CurrentAnnualDividend <= 0 || data.Dividend.PayoutRatio <= 0)
            {
                ddmWeight = 0m;
            }
            else
            {
                // Pro vysoké dividendové výnosy zvýšíme váhu DDM
                if (data.Dividend.DividendYield > 0.04m) // > 4% dividendový výnos
                {
                    ddmWeight += 0.1m;
                    dcfWeight -= 0.05m;
                    compWeight -= 0.05m;
                }
            }
            
            // 2. Pokud má společnost záporné zisky, snížíme váhu comparable analýzy
            if (data.Earnings.EPS <= 0)
            {
                compWeight *= 0.5m;
                dcfWeight += compWeight * 0.5m;
            }
            
            // 3. Pro rostoucí společnosti více zdůrazníme DCF a Score model
            if (data.Growth.PredictedEpsGrowth > 0.15m) // > 15% růst EPS
            {
                dcfWeight += 0.05m;
                scoreWeight += 0.05m;
                compWeight -= 0.05m;
                ddmWeight -= 0.05m;
            }
            
            // 4. Pro stabilní společnosti zdůrazníme comparable a DDM
            if (data.Growth.PredictedEpsGrowth < 0.05m && data.Stability.DebtToEquity < 1.0m) // pomalý růst a stabilní
            {
                compWeight += 0.05m;
                if (ddmWeight > 0) ddmWeight += 0.05m;
                dcfWeight -= 0.05m;
                scoreWeight -= 0.05m;
            }
            
            // 5. Pokud jsou některé metriky nevěrohodné, snížíme jejich váhu
            if (lastPrice > 0 && Math.Abs((dcfValue / lastPrice) - 1.0m) > 1.0m) // DCF se výrazně liší od aktuální ceny
            {
                dcfWeight *= 0.8m;
                compWeight += dcfWeight * 0.2m;
            }
            
            if (ddmWeight > 0 && lastPrice > 0 && (Math.Abs((ddmValue / lastPrice) - 1.0m) > 1.0m))
            {
                ddmWeight *= 0.8m;
                if (dcfWeight > 0) dcfWeight += ddmWeight * 0.2m;
            }
            
            // Normalizace vah, aby součet byl 1.0
            decimal totalWeight = dcfWeight + ddmWeight + compWeight + scoreWeight;
            if (totalWeight > 0)
            {
                dcfWeight /= totalWeight;
                ddmWeight /= totalWeight;
                compWeight /= totalWeight;
                scoreWeight /= totalWeight;
            }
            else
            {
                // Fallback váhy pokud by všechny byly 0
                dcfWeight = 0.5m;
                compWeight = 0.3m;
                scoreWeight = 0.2m;
            }
            
            // Výpočet vážené hodnoty
            decimal weightedValue = 
                (dcfValue * dcfWeight) + 
                (ddmValue * ddmWeight) + 
                (compValue * compWeight) + 
                (scoreValue * scoreWeight);
            
            // Kontrola konzistence a ošetření extrémních hodnot
            if (lastPrice > 0)
            {
                // Pokud je vážená hodnota příliš extrémní, přidáme omezení na max odchylku
                decimal maxDeviation = 2.0m; // max 200% odchylka
                decimal minDeviation = 0.3m; // min -70% odchylka
                
                decimal relativeDiff = weightedValue / lastPrice;
                
                if (relativeDiff > maxDeviation)
                {
                    weightedValue = lastPrice * maxDeviation;
                }
                else if (relativeDiff < minDeviation)
                {
                    weightedValue = lastPrice * minDeviation;
                }
            }
            
            // Zaokrouhlení na dvě desetinná místa pro přehlednost
            return Math.Round(weightedValue, 2);
        }
    }
}