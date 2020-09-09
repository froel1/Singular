using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gelf.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Winton.Extensions.Configuration.Consul;

namespace Singular.CorePlatform.Game.EGT
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    try
                    {
                        var integrationLabel = Environment.GetEnvironmentVariable("INTEGRATION_LABEL");
                        if (string.IsNullOrEmpty(integrationLabel))
                        {
                            Console.WriteLine("Environment variable INTEGRATION_LABEL is not set, exiting application");
                            Environment.Exit(-1);
                        }

                        var consulAddress = Environment.GetEnvironmentVariable("CONSUL_SERVICE_URL");
                        if (string.IsNullOrEmpty(consulAddress))
                        {
                            Console.WriteLine("Environment variable CONSUL_SERVICE_URL is not set, exiting application");
                            Environment.Exit(-1);
                        }

                        config.AddConsul("core-platform/common", options =>
                        {
                            options.ReloadOnChange = true;
                            options.ConsulConfigurationOptions = cco => cco.Address = new Uri(consulAddress);
                            options.OnLoadException = exceptionContext =>
                            {
                                Console.WriteLine($"Exception: {exceptionContext.Exception.Message}");
                                Console.WriteLine($"Error loading common configuration from Consul [url: {consulAddress}], exiting application.");
                                Environment.Exit(-1);
                            };
                        });
                        
                        config.AddConsul($"core-platform/integrations/{integrationLabel}", options =>
                        {
                            options.ReloadOnChange = true;
                            options.ConsulConfigurationOptions = cco => cco.Address = new Uri(consulAddress);
                            options.OnLoadException = exceptionContext =>
                            {
                                Console.WriteLine($"Exception: {exceptionContext.Exception.Message}");
                                Console.WriteLine($"Error loading integration [label: {integrationLabel}] configuration from Consul [url: {consulAddress}], exiting application");
                                Environment.Exit(-1);
                            };
                        });
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception: {ex}");
                        Console.WriteLine("Error during application configuration, exiting application.");
                        Environment.Exit(-1);
                    }
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddGelf(options =>
                    {
                        if (string.IsNullOrEmpty(options.Host))
                        {
                            Console.WriteLine("Error during application configuration: Graylog host is not configured, exiting application.");
                            Environment.Exit(-1);
                        }

                        options.LogSource = $"game-{hostingContext.Configuration["INTEGRATION_LABEL"]}";
                    });
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
