using System.Threading.Tasks;
using DanishAddressSeed.Dawa;
using FluentMigrator.Runner;
using Microsoft.Extensions.Logging;

namespace DanishAddressSeed
{
    internal class Startup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IMigrationRunner _migrationRunner;
        private readonly IClient _client;

        public Startup(
            ILogger<Startup> logger,
            IMigrationRunner migrationRunner,
            IClient client)
        {
            _logger = logger;
            _migrationRunner = migrationRunner;
            _client = client;
        }

        public async Task Start()
        {
            _logger.LogInformation($"Starting {nameof(DanishAddressSeed)}");
            _migrationRunner.MigrateUp();

            await _client.ImportOfficalAccessAddress();
            await _client.ImportOfficalUnitAddress();

            _logger.LogInformation($"Finished {nameof(DanishAddressSeed)}");
        }
    }
}
