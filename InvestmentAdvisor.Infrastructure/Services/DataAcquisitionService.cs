using System;
using System.Linq;
using System.Threading.Tasks;
using Domain.Interfaces;
using Infrastructure.Adapters.APIs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Infrastructure.Adapters.Mappers;

namespace Infrastructure.Services
{
    /// <summary>
    /// Implementace IDataAcquisitionService, která využívá externí API (AlphaVantage, Finnhub, IEXCloud)
    /// a repozitáře k aktualizaci a ukládání investičních dat do DB.
    /// </summary>
    public class DataAcquisitionService : IDataAcquisitionService
    {
        private readonly AlphaVantageClient _alphaVantageClient;
        private readonly FinnhubClient _finnhubClient;
        private readonly IEXCloudClient _iexCloudClient;
        private readonly IInvestmentInstrumentRepository _instrumentRepository;
        private readonly ILogger<DataAcquisitionService> _logger;

        // Konstruktor pomocí DI, získáváme implementace klientů pro API a repozitář.
        public DataAcquisitionService(
            AlphaVantageClient alphaVantageClient,
            FinnhubClient finnhubClient,
            IEXCloudClient iexCloudClient,
            IInvestmentInstrumentRepository instrumentRepository,
            ILogger<DataAcquisitionService> logger)
        {
            _alphaVantageClient = alphaVantageClient;
            _finnhubClient = finnhubClient;
            _iexCloudClient = iexCloudClient;
            _instrumentRepository = instrumentRepository;
            _logger = logger;
        }

        public async Task FullUpdateAsync(string symbol)
        {
            // Kompletní aktualizace všech dat pro daný symbol
            await UpdatePriceDataAsync(symbol);
            await UpdateFundamentalsAsync(symbol);
            await UpdateSentimentAndEstimatesAsync(symbol);
        }

        public async Task UpdatePriceDataAsync(string symbol)
        {
            try
            {
                var json = await _alphaVantageClient.GetDailyPricesAsync(symbol);
                var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("Time Series (Daily)", out var timeSeries))
                {
                    _logger.LogWarning("Time Series not found for {Symbol} in AlphaVantage response.", symbol);
                    return;
                }

                var instrument = await _instrumentRepository.GetBySymbolAsync(symbol);
                if (instrument == null)
                {
                    instrument = new Domain.Entities.InvestmentInstrument(symbol, symbol);
                    await _instrumentRepository.AddAsync(instrument);
                }

                foreach (var day in timeSeries.EnumerateObject())
                {
                    DateTime date = DateTime.Parse(day.Name);
                    var priceData = ExternalDataToDomainMapper.MapPriceData(day.Value, date);

                    // Přidáme data, pokud je ještě nemáme (např. kontrola duplicity by se mohla přidat)
                    instrument.AddPriceData(priceData);
                }

                await _instrumentRepository.UpdateAsync(instrument);
                _logger.LogInformation("Price data updated for {Symbol} from AlphaVantage.", symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update price data for {Symbol}.", symbol);
            }
        }

        public async Task UpdateFundamentalsAsync(string symbol)
        {
            try
            {
                var json = await _finnhubClient.GetCompanyProfileAsync(symbol);
                var doc = JsonDocument.Parse(json);

                var instrument = await _instrumentRepository.GetBySymbolAsync(symbol);
                if (instrument == null)
                {
                    instrument = new Domain.Entities.InvestmentInstrument(symbol, symbol);
                    await _instrumentRepository.AddAsync(instrument);
                }

                var fundamentals = ExternalDataToDomainMapper.MapFundamentalData(doc.RootElement);
                instrument.UpdateFundamentalData(fundamentals);

                await _instrumentRepository.UpdateAsync(instrument);
                _logger.LogInformation("Fundamentals updated for {Symbol} from Finnhub.", symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update fundamentals for {Symbol}.", symbol);
            }
        }

        public async Task UpdateSentimentAndEstimatesAsync(string symbol)
        {
            try
            {
                var json = await _iexCloudClient.GetQuoteAsync(symbol);
                var doc = JsonDocument.Parse(json);

                var instrument = await _instrumentRepository.GetBySymbolAsync(symbol);
                if (instrument == null)
                {
                    instrument = new Domain.Entities.InvestmentInstrument(symbol, symbol);
                    await _instrumentRepository.AddAsync(instrument);
                }

                var updatedFundamentals = ExternalDataToDomainMapper.MapSentimentAndEstimates(instrument.FundamentalData, doc.RootElement);
                instrument.UpdateFundamentalData(updatedFundamentals);

                await _instrumentRepository.UpdateAsync(instrument);
                _logger.LogInformation("Sentiment and estimates updated for {Symbol} from IEX Cloud.", symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update sentiment/estimates for {Symbol}.", symbol);
            }
        }
    }
}
