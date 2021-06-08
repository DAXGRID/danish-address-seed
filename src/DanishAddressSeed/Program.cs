﻿using System.IO;
using System.Threading.Tasks;
using DanishAddressSeed.Dawa;
using DanishAddressSeed.Location;
using DanishAddressSeed.Mapper;
using DanishAddressSeed.SchemaMigration;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;
using Serilog.Sinks.SystemConsole;

namespace DanishAddressSeed
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            var serviceProvider = BuildServiceProvider();
            var startup = serviceProvider.GetService<Startup>();

            await startup.Start();
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console(new CompactJsonFormatter())
                .CreateLogger();


            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .AddLogging(logging =>
                {
                    logging.AddSerilog(logger, true);
                })
                .AddSingleton<Startup>()
                .AddTransient<IClient, Client>()
                .AddTransient<ILocationPostgres, LocationPostgres>()
                .AddTransient<ILocationDawaMapper, LocationDawaMapper>()
                .AddHttpClient()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                                 .AddPostgres()
                                 .WithGlobalConnectionString(config.GetValue<string>("CONNECTION_STRING"))
                                 .ScanIn(typeof(InitialLocationSchemaSetup).Assembly).For.Migrations())
                .BuildServiceProvider();

            return serviceProvider;
        }
    }
}
