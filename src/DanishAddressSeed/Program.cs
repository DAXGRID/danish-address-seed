using System.IO;
using System.Threading.Tasks;
using DanishAddressSeed.Dawa;
using DanishAddressSeed.Location;
using DanishAddressSeed.Mapper;
using DanishAddressSeed.SchemaMigration;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .AddLogging(x => x.AddConsole())
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
