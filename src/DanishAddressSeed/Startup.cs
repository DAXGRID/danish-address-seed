using System;
using System.Collections.Generic;
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
        private readonly IDawaClient _client;
        private readonly ILocationPostgres _locationPostgres;

        public Startup(
            ILogger<Startup> logger,
            IMigrationRunner migrationRunner,
            IDawaClient client,
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
                await BulkInsertAccessAddresses(newTransactionId);
                await BulkInsertUnitAddresses(newTransactionId);
            }
            else
            {
                _logger.LogInformation($"Getting latest changes using existing tid {lastTransactionId} and new tid {newTransactionId}");
                await UpdateAccessAddresses(lastTransactionId, newTransactionId);
                await UpdateUnitAddresses(lastTransactionId, newTransactionId);
            }

            _logger.LogInformation($"Insert new transaction history with transaction_id '{newTransactionId}'");
            await _locationPostgres.InsertTransactionHistory(newTransactionId, DateTime.UtcNow);

            _logger.LogInformation($"Finished {nameof(DanishAddressSeed)}");
        }

        private async Task UpdateAccessAddresses(string fromTransId, string toTransId)
        {
            var count = 0;
            await foreach (var changeEvent in _client.RetrieveChangesOfficalAccessAddress(fromTransId, toTransId))
            {
                count++;
                switch (changeEvent.Operation)
                {
                    case "update":
                        await _locationPostgres.UpdateOfficalAccessAddress(changeEvent.Data);
                        break;
                    case "insert":
                        await _locationPostgres.InsertOfficalAccessAddresses(new List<OfficialAccessAddress> { changeEvent.Data });
                        break;
                    case "delete":
                        await _locationPostgres.UpdateOfficalAccessAddress(changeEvent.Data);
                        break;
                    default:
                        throw new Exception($"Operation '{changeEvent.Operation}' is not implemented for AccessAddress");
                }
            }

            _logger.LogInformation($"Created/Updated/Deleted '{count} OfficialAccessAddresses");
        }

        private async Task UpdateUnitAddresses(string fromTransId, string toTransId)
        {
            var count = 0;
            await foreach (var changeEvent in _client.RetrieveChangesOfficialUnitAddress(fromTransId, toTransId))
            {
                count++;
                switch (changeEvent.Operation)
                {
                    case "update":
                        await _locationPostgres.UpdateOfficialUnitAddress(changeEvent.Data);
                        break;
                    case "insert":
                        await _locationPostgres.InsertOfficialUnitAddresses(new List<OfficialUnitAddress> { changeEvent.Data });
                        break;
                    case "delete":
                        await _locationPostgres.UpdateOfficialUnitAddress(changeEvent.Data);
                        break;
                    default:
                        throw new Exception($"Operation '{changeEvent.Operation}' is not implemented for AccessAddress");
                }
            }

            _logger.LogInformation($"Created/Updated/Deleted '{count} OfficialUnitAddresses");
        }

        private async Task BulkInsertAccessAddresses(string newTransactionId)
        {
            var accessAddresses = new List<OfficialAccessAddress>();
            var count = 0;
            await foreach (var accessAddress in _client.RetrieveAllOfficialAccessAddresses(newTransactionId))
            {
                accessAddresses.Add(accessAddress);

                if (accessAddresses.Count == 5000)
                {
                    count += 5000;
                    _logger.LogInformation($"Imported: {count}");
                    await _locationPostgres.InsertOfficalAccessAddresses(accessAddresses);
                    accessAddresses.Clear();
                }
            }

            await _locationPostgres.InsertOfficalAccessAddresses(accessAddresses);
        }

        private async Task BulkInsertUnitAddresses(string newTransactionId)
        {
            var unitAddresses = new List<OfficialUnitAddress>();
            var count = 0;
            await foreach (var unitAddress in _client.RetrieveAllOfficalUnitAddresses(newTransactionId))
            {
                unitAddresses.Add(unitAddress);

                if (unitAddresses.Count == 5000)
                {
                    count += 5000;
                    _logger.LogInformation($"Imported: {count}");
                    await _locationPostgres.InsertOfficialUnitAddresses(unitAddresses);
                    unitAddresses.Clear();
                }
            }

            await _locationPostgres.InsertOfficialUnitAddresses(unitAddresses);
        }
    }
}
