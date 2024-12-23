using Domain.Interfaces;
using Domain.Services;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DependencyInjection
{
    public static class InfrastructureServicesRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrace AppDbContext
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            // Registrace repozitářů
            services.AddScoped<IInvestmentInstrumentRepository, InvestmentInstrumentRepository>();

            // Registrace doménových služeb
            services.AddScoped<IValuationService, ValuationService>();
            services.AddScoped<IRecommendationService, RecommendationService>();

            // Registrace dalších služeb
            services.AddScoped<IDataAcquisitionService, DataAcquisitionService>();

            return services;
        }
    }
}
