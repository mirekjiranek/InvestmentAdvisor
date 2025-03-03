using Domain.Interfaces;
using Domain.Services;
using Infrastructure.Adapters.APIs;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Infrastructure.Services.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.DependencyInjection
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register cache settings and services
            services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));
            services.AddMemoryCache();
            services.AddScoped<ICachingService, MemoryCachingService>();

            // Register AppDbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Register repositories
            services.AddScoped<IInvestmentInstrumentRepository, InvestmentInstrumentRepository>();

            // Register domain services
            services.AddScoped<IValuationService, ValuationService>();
            services.AddScoped<IRecommendationService, RecommendationService>();

            // Get policy configuration
            var retryCount = configuration.GetValue<int>("PolicyConfig:RetryCount");
            var breakDurationInSeconds = configuration.GetValue<int>("PolicyConfig:BreakDuration");

            // Configure API clients with retry and circuit breaker manually
            services.AddHttpClient<IAlphaVantageClient, AlphaVantageClient>(client =>
            {
                client.BaseAddress = new Uri("https://www.alphavantage.co/");
            }).ConfigurePrimaryHttpMessageHandler(() => new ResilientHttpClientHandler(retryCount, breakDurationInSeconds));

            services.AddHttpClient<IFinnhubClient, FinnhubClient>(client =>
            {
                client.BaseAddress = new Uri("https://finnhub.io/api/v1/");
            }).ConfigurePrimaryHttpMessageHandler(() => new ResilientHttpClientHandler(retryCount, breakDurationInSeconds));

            services.AddHttpClient<IIEXCloudClient, IEXCloudClient>(client =>
            {
                client.BaseAddress = new Uri("https://cloud.iexapis.com/stable/");
            }).ConfigurePrimaryHttpMessageHandler(() => new ResilientHttpClientHandler(retryCount, breakDurationInSeconds));

            // Register other services
            services.AddScoped<IDataAcquisitionService, DataAcquisitionService>();

            return services;
        }
    }

    public class ResilientHttpClientHandler : HttpClientHandler
    {
        private readonly int _retryCount;
        private readonly TimeSpan _breakDuration;
        private int _failureCount = 0;
        private readonly SemaphoreSlim _circuitBreaker = new(1, 1);
        private DateTime _lastFailureTime = DateTime.MinValue;

        public ResilientHttpClientHandler(int retryCount, int breakDurationInSeconds)
        {
            _retryCount = retryCount;
            _breakDuration = TimeSpan.FromSeconds(breakDurationInSeconds);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Circuit breaker: pokud došlo k mnoha chybám, zablokujeme požadavky na urèitý èas
            if (_failureCount >= 5 && (DateTime.UtcNow - _lastFailureTime) < _breakDuration)
            {
                throw new HttpRequestException("Circuit breaker activated. Skipping request.");
            }

            for (int i = 0; i < _retryCount; i++)
            {
                try
                {
                    HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _failureCount = 0; // Reset failure counter on success
                        return response;
                    }
                }
                catch (HttpRequestException)
                {
                    _failureCount++;
                    _lastFailureTime = DateTime.UtcNow;

                    // Pokud jsme dosáhli limitu chyb, aktivujeme circuit breaker
                    if (_failureCount >= 5)
                    {
                        return new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable)
                        {
                            Content = new StringContent("Service is temporarily unavailable due to repeated failures.")
                        };
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)), cancellationToken); // Exponential backoff
            }

            throw new HttpRequestException("Max retries reached.");
        }
    }
}
