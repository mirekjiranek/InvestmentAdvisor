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
using Polly;
using Polly.Extensions.Http;

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

            // Configure API clients with HttpClient factory
            services.AddHttpClient<IAlphaVantageClient, AlphaVantageClient>(client => 
            {
                client.BaseAddress = new Uri("https://www.alphavantage.co/");
            }).AddPolicyHandler(GetRetryPolicy(retryCount))
              .AddPolicyHandler(GetCircuitBreakerPolicy(breakDurationInSeconds));
            
            services.AddHttpClient<IFinnhubClient, FinnhubClient>(client => 
            {
                client.BaseAddress = new Uri("https://finnhub.io/api/v1/");
            }).AddPolicyHandler(GetRetryPolicy(retryCount))
              .AddPolicyHandler(GetCircuitBreakerPolicy(breakDurationInSeconds));
            
            services.AddHttpClient<IIEXCloudClient, IEXCloudClient>(client => 
            {
                client.BaseAddress = new Uri("https://cloud.iexapis.com/stable/");
            }).AddPolicyHandler(GetRetryPolicy(retryCount))
              .AddPolicyHandler(GetCircuitBreakerPolicy(breakDurationInSeconds));

            // Register other services
            services.AddScoped<IDataAcquisitionService, DataAcquisitionService>();

            return services;
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int breakDurationInSeconds)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(breakDurationInSeconds));
        }
    }
}