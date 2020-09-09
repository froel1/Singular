using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Singular.CorePlatform.Adapter;
using Singular.CorePlatform.Adapter.Interfaces;
using Singular.CorePlatform.Common;
using Singular.CorePlatform.Common.Configuration;
using Singular.CorePlatform.Common.DiagnosticsTools;
using Singular.CorePlatform.Common.Middlewares;
using Singular.CorePlatform.Common.Versioning;
using Singular.CorePlatform.FreeSpins.Abstracts;
using Singular.CorePlatform.Game.EGT.Core.Interfaces;
using Singular.CorePlatform.Game.EGT.Core.Services;
using Singular.CorePlatform.Game.EGT.Filters;
using Singular.CorePlatform.Game.EGT.Models;
using Singular.CorePlatform.Games;
using Singular.CorePlatform.Games.Interfaces;
using Singular.CorePlatform.Persistence.Interfaces;
using Singular.CorePlatform.Persistence.Token;

namespace Singular.CorePlatform.Game.EGT
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
            foreach (var instance in Configuration.GetSection("Instances").GetChildren())
            {
                services.Configure<IntegrationOptions>(instance.Key, instance);
                services.AddOptions<EGTSettings>(instance.Key)
                    .Bind(instance)
                    .Validate(x => x.IsValid());
            }

            services.AddControllers()
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                    {
                        var message = string.Join("\n", context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                        return new BadRequestObjectResult(new OperationResult(ErrorCodes.WrongRequest, message));
                    };
                })
                .AddNewtonsoftJson();
            services.AddApiVersioning(options => options.ErrorResponses = new ApiVersioningErrorResponseProvider());
            services.AddHttpContextAccessor();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "EGT Game Integration API", Version = "v1" });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddSwaggerGenNewtonsoftSupport();

            services.AddScoped<IIntegrationInstance, IntegrationInstance>();
            services.AddScoped<IIntegrationService, IntegrationService>();
            services.AddScoped<IGameDirectory, GameDirectory>();
            services.AddScoped<ICasinoUserAdapter, CasinoUserAdapter>();
            services.AddScoped<IPayAdapter, PayAdapter>();
            services.AddScoped<IFreeSpins, FreeSpins.FreeSpins>();
            services.AddScoped<ITokenFactory, TokenFactory>();

            services.AddScoped<CheckIpAttribute>();
            services.AddScoped<CheckCredentialsAttribute>();
            services.AddScoped<HandleExceptionAttribute>();

            services.RegisterHealthMonitoringServices(Configuration)
                .AddBaseService(Configuration, BaseService.GameDirectory)
                .AddBaseService(Configuration, BaseService.GameAdapter)
                .AddBaseService(Configuration, BaseService.GamePersistence);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHealthMonitoring();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "EGT Game Integration API V1");
            });

            // TODO add regex patterns to hide sensitive information in logs
            app.UseRequestResponseLogging(options =>
            {
                options.Hides = new Regex[] { };
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        context.Response.ContentType = "application/json";

                        var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
                        var exceptionMessage = exceptionHandlerFeature?.Error.Message ?? "Something went wrong";

                        var result = new OperationResult(ErrorCodes.GenericFailed, exceptionMessage);
                        var response = JsonConvert.SerializeObject(result);
                        await context.Response.WriteAsync(response);
                    });
                });
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
