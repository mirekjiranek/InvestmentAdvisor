using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Commands;
using Application.DTOs;
using Application.Queries;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Domain.Interfaces;
using Domain.Services;

using Microsoft.Extensions.Configuration;

namespace Application.DependencyInjection
{
    public static class ApplicationServicesRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Registrace command handleru:
            services.AddScoped<ICommandHandler<UpdateInstrumentsCommand>,
                               Application.Commands.UpdateInstrumentsCommandHandler>();

            // Registrace query handleru:
            services.AddScoped<IQueryHandler<Application.Queries.GetInstrumentQuery, InstrumentDto?>,
                               Application.Queries.GetInstrumentQueryHandler>();

            return services;
        }
    }
}
