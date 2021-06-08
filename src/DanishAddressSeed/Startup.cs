using System;
using System.Threading.Tasks;
using DanishAddressSeed.Dawa;
using DanishAddressSeed.Location;
using FluentMigrator.Runner;
using Microsoft.Extensions.Logging;

namespace DanishAddressSeed
{
    internal class Startup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IMigrationRunner _migrationRunner;
        private readonly IClient _client;
        private readonly ILocationPostgres _locationPostgres;

        public Startup(
            ILogger<Startup> logger,
            IMigrationRunner migrationRunner,
            IClient client,
            ILocationPostgres locationPostgres = null)
        {
            _logger = logger;
            _migrationRunner = migrationRunner;
            _client = client;
            _locationPostgres = locationPostgres;
        }

        public async Task Start()
        {
            _logger.LogInformation($"Starting {nameof(DanishAddressSeed)}");
            _migrationRunner.MigrateUp();

            var lastTransactionId = await _locationPostgres.GetLatestTransactionHistory();
            var newTransactionId = await _client.GetTransactionId();

            if (string.IsNullOrEmpty(lastTransactionId))
            {
                _logger.LogInformation("First time running, doing full bulk import");
                await _client.BulkOfficalAccessAddress(newTransactionId);
                await _client.BulkImportOfficalUnitAddress(newTransactionId);
            }
            else
            {
                _logger.LogInformation("Updating existing unit and access addresses");
                await _client.UpdateOfficalAccessAddress(lastTransactionId, newTransactionId);
                await _client.UpdateOfficialUnitAddress(lastTransactionId, newTransactionId);
            }

            _logger.LogInformation($"Insert new transaction history with transaction_id '{newTransactionId}'");
            await _locationPostgres.InsertTransactionHistory(newTransactionId, DateTime.UtcNow);

            _logger.LogInformation($"Finished {nameof(DanishAddressSeed)}");
        }
    }
}
