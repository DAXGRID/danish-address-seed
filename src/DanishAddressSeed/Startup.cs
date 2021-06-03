using FluentMigrator.Runner;
using Microsoft.Extensions.Logging;

namespace DanishAddressSeed
{
    internal class Startup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IMigrationRunner _migrationRunner;

        public Startup(ILogger<Startup> logger, IMigrationRunner migrationRunner)
        {
            _logger = logger;
            _migrationRunner = migrationRunner;
        }

        public void Start()
        {
            _logger.LogInformation($"Starting {nameof(DanishAddressSeed)}");
            _migrationRunner.MigrateUp();
        }
    }
}
