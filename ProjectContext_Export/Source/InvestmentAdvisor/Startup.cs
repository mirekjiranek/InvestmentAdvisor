using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Domain.Services;
using Infrastructure.Services;
using Infrastructure.Adapters.APIs;
using Infrastructure.Adapters.APIs.Policies; // Přidejte tento using
using Application;
using Application.DependencyInjection;
using Polly.Extensions.Http;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly;
using Polly.Extensions.Http; // Namespace, kde je ApplicationServicesRegistration
// Předpokládáme, že `AddApplicationServices()` je v InvestmentAdvisor.Application

namespace InvestmentAdvisor.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 1. Nastavení DB Contextu s použitím PostgreSQL
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));

            // 2. Registrace repozitářů
            services.AddScoped<IInvestmentInstrumentRepository, InvestmentInstrumentRepository>();

            // 3. Registrace doménových služeb (Valuation, Recommendation)
            services.AddScoped<IValuationService, ValuationService>();
            services.AddScoped<IRecommendationService, RecommendationService>();

            // 4. Registrace služeb z Infrastructure
            services.AddScoped<IDataAcquisitionService, DataAcquisitionService>();

            // 5. Registrace HTTP klientů pro externí API s Polly politikami

            // 6. Registrace Application služeb (CQRS Handlery)
            services.AddApplicationServices();

            // 7. Přidání Controllerů + případně Blazor/Razor pages
            services.AddControllers();
            services.AddServerSideBlazor();

            // 8. Swagger pro dokumentaci API
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // 9. Developer nastavení a Swagger v Development režimu
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "InvestmentAdvisor API v1"));
            }

            // 10. Routing
            app.UseRouting();

            // 11. Authorization (pokud je potřeba, jinak lze vynechat)
            app.UseAuthorization();

            // 12. Mapování endpointů: Kontrolery, BlazorHub
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }

}
