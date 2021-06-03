using System;
using System.IO;
using DanishAddressSeed.SchemaMigration;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DanishAddressSeed
{
    class Program
    {
        static void Main(string[] args)
        {
            var root = Directory.GetCurrentDirectory();
            var dotenv = Path.Combine(root, ".env");
            DotEnv.Load(dotenv);

            var serviceProvider = BuildServiceProvider();
            var startup = serviceProvider.GetService<Startup>();

            startup.Start();
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfigurationRoot>(config)
                .AddLogging(x => x.AddConsole())
                .AddSingleton<Startup>()
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
