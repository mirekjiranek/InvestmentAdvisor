using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Domain.Interfaces;
using Infrastructure.Adapters.APIs;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Infrastructure.Adapters.Mappers;
using Infrastructure.Services.Caching;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    /// <summary>
    /// Implementace IDataAcquisitionService, která využívá externí API (AlphaVantage, Finnhub, IEXCloud)
    /// a repozitáře k aktualizaci a ukládání investičních dat do DB.
    /// </summary>
    public class DataAcquisitionService : IDataAcquisitionService
    {
        private readonly IAlphaVantageClient _alphaVantageClient;
        private readonly IFinnhubClient _finnhubClient;
        private readonly IIEXCloudClient _iexCloudClient;
        private readonly IInvestmentInstrumentRepository _instrumentRepository;
        private readonly ILogger<DataAcquisitionService> _logger;
        private readonly ICachingService _cachingService;
        private readonly CacheSettings _cacheSettings;

        // Konstruktor pomocí DI, získáváme implementace klientů pro API a repozitář.
        public DataAcquisitionService(
            IAlphaVantageClient alphaVantageClient,
            IFinnhubClient finnhubClient,
            IIEXCloudClient iexCloudClient,
            IInvestmentInstrumentRepository instrumentRepository,
            ILogger<DataAcquisitionService> logger,
            ICachingService cachingService,
            IOptions<CacheSettings> cacheSettings)
        {
            _alphaVantageClient = alphaVantageClient;
            _finnhubClient = finnhubClient;
            _iexCloudClient = iexCloudClient;
            _instrumentRepository = instrumentRepository;
            _logger = logger;
            _cachingService = cachingService;
            _cacheSettings = cacheSettings.Value;
        }

        public async Task FullUpdateAsync(string symbol, CancellationToken cancellationToken = default)
        {
            // Kompletní aktualizace všech dat pro daný symbol
            await UpdatePriceDataAsync(symbol, cancellationToken);
            await UpdateFundamentalsAsync(symbol, cancellationToken);
            await UpdateSentimentAndEstimatesAsync(symbol, cancellationToken);
        }

        public async Task UpdateInstrumentsDataAsync(List<string> symbols, CancellationToken cancellationToken = default)
        {
            foreach (var symbol in symbols)
            {
                await FullUpdateAsync(symbol, cancellationToken);
            }
        }

        public async Task UpdatePriceDataAsync(string symbol, CancellationToken cancellationToken = default)
        {
            try
            {
                string cacheKey = $"price_data_{symbol}";
                var json = await _cachingService.GetOrCreateAsync(
                    cacheKey, 
                    () => _alphaVantageClient.GetDailyPricesAsync(symbol),
                    TimeSpan.FromMinutes(_cacheSettings.PriceDataExpirationMinutes));
                
                var doc = JsonDocument.Parse(json);

                if (!doc.RootElement.TryGetProperty("Time Series (Daily)", out var timeSeries))
                {
                    _logger.LogWarning("Time Series not found for {Symbol} in AlphaVantage response.", symbol);
                    return;
                }

                var instrument = await _instrumentRepository.GetBySymbolAsync(symbol, cancellationToken);
                if (instrument == null)
                {
                    instrument = new Domain.Entities.InvestmentInstrument(symbol, symbol);
                    await _instrumentRepository.AddAsync(instrument, cancellationToken);
                }

                foreach (var day in timeSeries.EnumerateObject())
                {
                    DateTime date = DateTime.Parse(day.Name);
                    var priceData = ExternalDataToDomainMapper.MapPriceData(day.Value, date);

                    // Přidáme data, pokud je ještě nemáme (např. kontrola duplicity by se mohla přidat)
                    instrument.AddPriceData(priceData);
                }

                await _instrumentRepository.UpdateAsync(instrument, cancellationToken);
                _logger.LogInformation("Price data updated for {Symbol} from AlphaVantage.", symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update price data for {Symbol}.", symbol);
            }
        }

        public async Task UpdateFundamentalsAsync(string symbol, CancellationToken cancellationToken = default)
        {
            try
            {
                string cacheKey = $"fundamentals_{symbol}";
                var json = await _cachingService.GetOrCreateAsync(
                    cacheKey, 
                    () => _finnhubClient.GetCompanyProfileAsync(symbol),
                    TimeSpan.FromMinutes(_cacheSettings.FundamentalsExpirationMinutes));
                
                var doc = JsonDocument.Parse(json);

                var instrument = await _instrumentRepository.GetBySymbolAsync(symbol, cancellationToken);
                if (instrument == null)
                {
                    instrument = new Domain.Entities.InvestmentInstrument(symbol, symbol);
                    await _instrumentRepository.AddAsync(instrument, cancellationToken);
                }

                var fundamentals = ExternalDataToDomainMapper.MapFundamentalData(doc.RootElement);
                instrument.UpdateFundamentalData(fundamentals);

                await _instrumentRepository.UpdateAsync(instrument, cancellationToken);
                _logger.LogInformation("Fundamentals updated for {Symbol} from Finnhub.", symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update fundamentals for {Symbol}.", symbol);
            }
        }

        public async Task UpdateSentimentAndEstimatesAsync(string symbol, CancellationToken cancellationToken = default)
        {
            try
            {
                string cacheKey = $"quotes_{symbol}";
                var json = await _cachingService.GetOrCreateAsync(
                    cacheKey, 
                    () => _iexCloudClient.GetQuoteAsync(symbol),
                    TimeSpan.FromMinutes(_cacheSettings.QuotesExpirationMinutes));
                
                var doc = JsonDocument.Parse(json);

                var instrument = await _instrumentRepository.GetBySymbolAsync(symbol, cancellationToken);
                if (instrument == null)
                {
                    instrument = new Domain.Entities.InvestmentInstrument(symbol, symbol);
                    await _instrumentRepository.AddAsync(instrument, cancellationToken);
                }

                var updatedFundamentals = ExternalDataToDomainMapper.MapSentimentAndEstimates(instrument.FundamentalData, doc.RootElement);
                instrument.UpdateFundamentalData(updatedFundamentals);

                await _instrumentRepository.UpdateAsync(instrument, cancellationToken);
                _logger.LogInformation("Sentiment and estimates updated for {Symbol} from IEX Cloud.", symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update sentiment/estimates for {Symbol}.", symbol);
            }
        }
    }
}