namespace Infrastructure.Services.Caching
{
    public class CacheSettings
    {
        public int DefaultExpirationMinutes { get; set; } = 60;
        public int PriceDataExpirationMinutes { get; set; } = 30;
        public int FundamentalsExpirationMinutes { get; set; } = 1440; // 24 hours
        public int QuotesExpirationMinutes { get; set; } = 15;
    }
}