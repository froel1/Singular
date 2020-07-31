using System;
using Balances;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Singular.Api.Helpers;
using Singular.Api.Interfaces;
using Singular.Api.Services;
using Singular.Api.Settings;

namespace Singular.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddTransient<CasinoBalanceManager>();
            services.AddTransient<GameBalanceManager>();
            services.AddTransient<IBalanceService, BalanceService>();

            services.AddTransient<ServiceResolver>(serviceProvider => key => key switch
            {
                BalanceSource.Casino => serviceProvider.GetService<CasinoBalanceManager>(),
                BalanceSource.Game => serviceProvider.GetService<GameBalanceManager>(),
                _ => throw new ArgumentOutOfRangeException(nameof(key), key, null)
            });

            services.AddSwaggerGen(opt =>
            {
                opt.SwaggerDoc("v1", new OpenApiInfo { Title = "Singular Api", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseMiddleware<ErrorHandlingMiddleware>();

            ConfigureSwagger(app);

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ConfigureSwagger(IApplicationBuilder app)
        {
            var swagger = new SwaggerOptions();
            Configuration.GetSection("Swagger").Bind(swagger);

            app.UseSwagger(options =>
            {
                options.RouteTemplate = swagger.JsonRoute;
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(swagger.UiEndpoint, swagger.Description);
            });
        }
    }
}