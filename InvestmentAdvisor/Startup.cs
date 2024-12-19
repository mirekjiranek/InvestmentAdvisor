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
using Application;
using Application.DependencyInjection; // Namespace, kde je ApplicationServicesRegistration
// Předpokládáme, že `AddApplicationServices()` je v InvestmentAdvisor.Application

namespace InvestmentAdvisor.Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            // Konfigurace ze souborů appsettings.json a jiných zdrojů
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // 1. Nastavení DB Contextu s použitím PostgreSQL
            // Connection string by měl být definován v appsettings.json pod "DefaultConnection"
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection")));

            // 2. Registrace repozitářů
            // Tyto repozitáře implementují rozhraní definovaná v Domain.Interfaces
            services.AddScoped<IInvestmentInstrumentRepository, InvestmentInstrumentRepository>();

            // 3. Registrace doménových služeb (Valuation, Recommendation)
            // IValuationService a IRecommendationService jsou v Domain.Interfaces,
            // implementace v Domain.Services či Infrastructure.
            services.AddScoped<IValuationService, ValuationService>();
            services.AddScoped<IRecommendationService, RecommendationService>();

            // 4. Registrace služeb z Infrastructure
            // IDataAcquisitionService je buď definované rozhraní v Domain/Application,
            // implementace v Infrastructure.Services.DataAcquisitionService.
            // Pokud rozhraní ještě není, doporučuje se jej vytvořit v Domain.Interfaces.
            services.AddScoped<IDataAcquisitionService, DataAcquisitionService>();

            // 5. Registrace HTTP klientů pro externí API
            // Můžeme použít pojmenované nebo typované klienty.
            services.AddHttpClient<AlphaVantageClient>();
            services.AddHttpClient<FinnhubClient>();
            services.AddHttpClient<IEXCloudClient>();

            // 6. Registrace Application služeb (CQRS Handlery)
            // AddApplicationServices zaregistruje command/query handlery a případná mapování.
            services.AddApplicationServices();

            // 7. Přidání Controllerů + případně Blazor/Razor pages
            services.AddControllers();
            // Odkomentovat, pokud bude UI přes Blazor
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
                endpoints.MapBlazorHub(); // Pokud je Blazor použit
                endpoints.MapFallbackToPage("/_Host"); // Fallback pro Blazor stránky
            });
        }
    }
}
