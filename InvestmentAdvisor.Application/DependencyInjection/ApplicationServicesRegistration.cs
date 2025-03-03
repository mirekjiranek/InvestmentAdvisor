using Application.Commands;
using Application.DTOs;
using Application.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Application.DependencyInjection
{
    public static class ApplicationServicesRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<ICommandHandler<UpdateInstrumentsCommand>, Application.Commands.UpdateInstrumentsCommandHandler>();
            services.AddScoped<IQueryHandler<Application.Queries.GetInstrumentQuery, InstrumentDto?>, Application.Queries.GetInstrumentQueryHandler>();
            return services;
        }

    }
}
