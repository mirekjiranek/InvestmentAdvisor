//using Domain.Entities;
//using Domain.Interfaces;
//using Domain.ValueObjects;
//using Infrastructure.Adapters.APIs;
//using Infrastructure.Services;
//using Infrastructure.Services.Caching;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Moq;
//using Xunit; // Ensure you have this for [Fact]

//namespace Tests
//{
//    public class DataAcquisitionServiceTests
//    {
//        private readonly Mock<IAlphaVantageClient> _alphaMock;
//        private readonly Mock<IFinnhubClient> _finnhubMock;
//        private readonly Mock<IIEXCloudClient> _iexMock;
//        private readonly Mock<IInvestmentInstrumentRepository> _repoMock;
//        private readonly Mock<ILogger<DataAcquisitionService>> _loggerMock;
//        private readonly Mock<ICachingService> _cacheMock;
//        private readonly Mock<IOptions<CacheSettings>> _cacheSettingsMock;
//        private readonly DataAcquisitionService _dataService;

//        public DataAcquisitionServiceTests()
//        {
//            _alphaMock = new Mock<IAlphaVantageClient>();
//            _finnhubMock = new Mock<IFinnhubClient>();
//            _iexMock = new Mock<IIEXCloudClient>();
//            _repoMock = new Mock<IInvestmentInstrumentRepository>();
//            _loggerMock = new Mock<ILogger<DataAcquisitionService>>();
//            _cacheMock = new Mock<ICachingService>();

//            // Mockování CacheSettings
//            var cacheSettings = new CacheSettings { ExpirationMinutes = 60 };
//            _cacheSettingsMock = new Mock<IOptions<CacheSettings>>();
//            _cacheSettingsMock.Setup(cs => cs.Value).Returns(cacheSettings);

//            _dataService = new DataAcquisitionService(
//                _alphaMock.Object,
//                _finnhubMock.Object,
//                _iexMock.Object,
//                _repoMock.Object,
//                _loggerMock.Object,
//                _cacheMock.Object,          // Přidání mocku ICachingService
//                _cacheSettingsMock.Object   // Přidání mocku IOptions<CacheSettings>
//            );
//        }

//        [Fact]
//        public async Task UpdatePriceDataAsync_InvalidResponse_GracefullyHandlesError()
//        {
//            string json = @"{ ""Error"": ""Invalid symbol"" }";
//            var instrument = new InvestmentInstrument("TEST", "Test Company");
//            _repoMock.Setup(r => r.GetBySymbolAsync("TEST")).ReturnsAsync(instrument);
//            _alphaMock.Setup(a => a.GetDailyPricesAsync("TEST", "compact")).ReturnsAsync(json);

//            await _dataService.UpdatePriceDataAsync("TEST");

//            Assert.Empty(instrument.PriceHistory);
//            _repoMock.Verify(r => r.UpdateAsync(instrument), Times.Never);
//        }

//        [Fact]
//        public async Task UpdateFundamentalsAsync_ValidData_MapsAndSavesFundamentals()
//        {
//            string json = @"{
//                ""peRatio"": 15.0,
//                ""pbRatio"": 2.0,
//                ""evToEbitda"": 10.0,
//                ""priceToSales"": 3.0,
//                ""epsGrowth5Y"": 0.05,
//                ""epsGrowth1Y"": 0.06,
//                ""revenueGrowth5Y"": 0.03,
//                ""returnOnEquity"": 0.15,
//                ""returnOnAssets"": 0.10,
//                ""grossMargin"": 0.40,
//                ""operatingMargin"": 0.20,
//                ""netProfitMargin"": 0.10,
//                ""debtToEquity"": 0.5,
//                ""currentRatio"": 1.5,
//                ""interestCoverage"": 4.0,
//                ""dividendYield"": 0.03,
//                ""payoutRatio"": 0.5,
//                ""annualDividend"": 2.0,
//                ""beta"": 1.0,
//                ""targetPrice"": 120.0,
//                ""analystRecommendation"": ""Buy"",
//                ""sectorPE"": 14,
//                ""sectorPB"": 2,
//                ""sectorEVEBITDA"": 12,
//                ""sectorPriceSales"": 3,
//                ""eps"": 5,
//                ""ebitda"": 500,
//                ""salesPerShare"": 50,
//                ""freeCashFlow"": 100
//            }";

//            var instrument = new InvestmentInstrument("TEST", "Test Company");
//            _repoMock.Setup(r => r.GetBySymbolAsync("TEST")).ReturnsAsync(instrument);
//            _finnhubMock.Setup(f => f.GetCompanyProfileAsync("TEST")).ReturnsAsync(json);

//            await _dataService.UpdateFundamentalsAsync("TEST");

//            Assert.NotNull(instrument.FundamentalData);
//            Assert.Equal(15.0m, instrument.FundamentalData.Valuation.PE);
//            _repoMock.Verify(r => r.UpdateAsync(instrument), Times.Once);
//        }

//        [Fact]
//        public async Task UpdateSentimentAndEstimatesAsync_ValidData_UpdatesSentiment()
//        {
//            string json = @"{
//                ""targetPrice"": 130.0,
//                ""recommendation"": ""StrongBuy"",
//                ""mediaSentiment"": 0.9
//            }";

//            var fundamentalData = new FundamentalData(
//                new ValuationMetrics(10, 2, 8, 7, 2, 10),
//                new GrowthMetrics(0.05m, 0.05m, 0.05m, 0.05m, 0.02m, 0.05m, 0.03m),
//                new ProfitabilityMetrics(0.15m, 0.10m, 0.40m, 0.20m, 0.10m),
//                new StabilityMetrics(0.5m, 1.5m, 1.2m, 4m),
//                new DividendMetrics(0.03m, 0.5m, 0.02m, 2m),
//                new MarketRiskMetrics(1.0m, 0.5m, 0.2m),
//                new SentimentMetrics(120m, "Buy", 0.8m),
//                new ComparableMetrics(15m, 2m, 12m, 3m),
//                new EarningsMetrics(5m, 500m),
//                new RevenueMetrics(50m),
//                new CashFlowMetrics(100m, 0.05m),
//                new CostOfCapitalMetrics(0.08m, 0.10m)
//            );

//            var instrument = new InvestmentInstrument("TEST", "Test Company");
//            instrument.UpdateFundamentalData(fundamentalData);

//            _repoMock.Setup(r => r.GetBySymbolAsync("TEST")).ReturnsAsync(instrument);
//            _iexMock.Setup(i => i.GetQuoteAsync("TEST")).ReturnsAsync(json);

//            await _dataService.UpdateSentimentAndEstimatesAsync("TEST");

//            Assert.Equal(130m, instrument.FundamentalData.Sentiment.ConsensusTargetPrice);
//            Assert.Equal("StrongBuy", instrument.FundamentalData.Sentiment.AnalystRecommendation);
//            Assert.Equal(0.9m, instrument.FundamentalData.Sentiment.MediaSentimentScore);
//            _repoMock.Verify(r => r.UpdateAsync(instrument), Times.Once);
//        }

//        [Fact]
//        public async Task UpdatePriceDataAsync_InvalidResponse_GracefullyHandlesError()
//        {
//            string json = @"{ ""Error"": ""Invalid symbol"" }";
//            var instrument = new InvestmentInstrument("TEST", "Test Company");
//            _repoMock.Setup(r => r.GetBySymbolAsync("TEST")).ReturnsAsync(instrument);
//            _alphaMock.Setup(a => a.GetDailyPricesAsync("TEST", "compact")).ReturnsAsync(json);

//            await _dataService.UpdatePriceDataAsync("TEST");

//            Assert.Empty(instrument.PriceHistory);
//            _repoMock.Verify(r => r.UpdateAsync(instrument), Times.Never);
//        }
//    }
//}
