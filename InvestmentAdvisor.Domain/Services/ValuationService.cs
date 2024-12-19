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

            // V reálu byste mohli nejprve zkontrolovat, zda firma vyplácí dividendy. 
            // Pokud ne, váha DDM modelu by mohla být menší nebo nulová.
            // Rovněž byste mohli nastavit váhy jednotlivých modelů.

            decimal dcfValue = _dcfModel.CalculateDCFValue(data);

            // Pokud firma nevyplácí dividendu, DDM dá menší význam:
            decimal ddmValue = data.Dividend.CurrentAnnualDividend > 0 ? _ddmModel.CalculateDDMValue(data) : dcfValue;

            decimal compValue = _comparableModel.CalculateComparableValue(data);
            decimal scoreValue = _scoreModel.CalculateScoreValue(data);

            // Předpokládejme váhy pro každý model (reálně by se daly konfigurovat)
            decimal dcfWeight = 0.4m;
            decimal ddmWeight = data.Dividend.CurrentAnnualDividend > 0 ? 0.2m : 0.0m;
            decimal compWeight = 0.2m;
            decimal scoreWeight = 0.2m;

            // Normalizace vah, pokud nejsou 1.0
            decimal totalWeight = dcfWeight + ddmWeight + compWeight + scoreWeight;
            dcfWeight /= totalWeight;
            ddmWeight /= totalWeight;
            compWeight /= totalWeight;
            scoreWeight /= totalWeight;

            decimal weightedValue = (dcfValue * dcfWeight) + (ddmValue * ddmWeight) + (compValue * compWeight) + (scoreValue * scoreWeight);

            return weightedValue;
        }
    }
}
