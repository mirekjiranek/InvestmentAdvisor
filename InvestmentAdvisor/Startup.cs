using Application.DependencyInjection;
using Infrastructure.DependencyInjection;
using InvestmentAdvisor.Web.Middleware;

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
            // Registrace aplikačních služeb
            services.AddApplicationServices();

            // Registrace infrastrukturních služeb
            services.AddInfrastructureServices(Configuration);

            // Další registrace (Controllers, Swagger, Blazor)
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "InvestmentAdvisor API v1"));
            }
            else
            {
                // Use custom exception handling middleware in non-development environments
                app.UseExceptionHandling();
            }

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }

}