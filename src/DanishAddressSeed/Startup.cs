using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DanishAddressSeed.Dawa;
using DanishAddressSeed.Location;
using FluentMigrator.Runner;
using Microsoft.Extensions.Logging;
using Typesense;

namespace DanishAddressSeed
{
    internal class Startup
    {
        private readonly ILogger<Startup> _logger;
        private readonly IMigrationRunner _migrationRunner;
        private readonly IDawaClient _client;
        private readonly ILocationPostgres _locationPostgres;
        private readonly ITypesenseClient _typesenseClient;
        private const Int16 ImportBatchCount = 500;

        public Startup(
            ILogger<Startup> logger,
            IMigrationRunner migrationRunner,
            IDawaClient client,
            ILocationPostgres locationPostgres = null,
            ITypesenseClient typesenseClient = null)
        {
            _logger = logger;
            _migrationRunner = migrationRunner;
            _client = client;
            _locationPostgres = locationPostgres;
            _typesenseClient = typesenseClient;
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
                await SetupTypesense();
                await BulkInsertAccessAddresses(newTransactionId);
                await BulkInsertUnitAddresses(newTransactionId);
            }
            else
            {
                _logger.LogInformation(
                    $"Getting latest changes using existing tid {lastTransactionId} and new tid {newTransactionId}");
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
                        await _locationPostgres.InsertOfficalAccessAddresses(
                            new List<OfficialAccessAddress> { changeEvent.Data });
                        break;
                    case "delete":
                        await _locationPostgres.UpdateOfficalAccessAddress(changeEvent.Data);
                        break;
                    default:
                        throw new Exception(
                            $"Operation '{changeEvent.Operation}' is not implemented for AccessAddress");
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
                        await _locationPostgres.InsertOfficialUnitAddresses(
                            new List<OfficialUnitAddress> { changeEvent.Data });
                        break;
                    case "delete":
                        await _locationPostgres.UpdateOfficialUnitAddress(changeEvent.Data);
                        break;
                    default:
                        throw new Exception(
                            $"Operation '{changeEvent.Operation}' is not implemented for AccessAddress");
                }
            }

            _logger.LogInformation($"Created/Updated/Deleted '{count} OfficialUnitAddresses");
        }

        private async Task BulkInsertAccessAddresses(string newTransactionId)
        {
            var addresses = new List<OfficialAccessAddress>();
            var count = 0;
            await foreach (var accessAddress in _client.RetrieveAllOfficialAccessAddresses(newTransactionId))
            {
                addresses.Add(accessAddress);

                if (addresses.Count == ImportBatchCount)
                {
                    count += addresses.Count;
                    _logger.LogInformation($"Imported: {count}");
                    var postgresTask = _locationPostgres.InsertOfficalAccessAddresses(addresses);
                    var typesenseTask = _typesenseClient.ImportDocuments<OfficialAccessAddress>(
                        "Addresses", addresses, ImportBatchCount, ImportType.Upsert);

                    Task.WaitAll(postgresTask, typesenseTask);
                    addresses.Clear();
                }
            }

            await _locationPostgres.InsertOfficalAccessAddresses(addresses);
            await _typesenseClient.ImportDocuments<OfficialAccessAddress>(
                "Addresses", addresses, addresses.Count, ImportType.Upsert);
        }

        private async Task BulkInsertUnitAddresses(string newTransactionId)
        {
            var addresses = new List<OfficialUnitAddress>();
            var count = 0;
            await foreach (var unitAddress in _client.RetrieveAllOfficalUnitAddresses(newTransactionId))
            {
                addresses.Add(unitAddress);

                if (addresses.Count == ImportBatchCount)
                {
                    count += addresses.Count;
                    _logger.LogInformation($"Imported: {count}");
                    var postgresTask = _locationPostgres.InsertOfficialUnitAddresses(addresses);
                    var typesenseTask = _typesenseClient.ImportDocuments<OfficialUnitAddress>(
                        "Addresses", addresses, ImportBatchCount, ImportType.Upsert);

                    Task.WaitAll(postgresTask, typesenseTask);
                    addresses.Clear();
                }
            }

            await _locationPostgres.InsertOfficialUnitAddresses(addresses);
            await _typesenseClient.ImportDocuments<OfficialUnitAddress>(
                "Addresses", addresses, addresses.Count, ImportType.Upsert);
        }

        private async Task SetupTypesense()
        {
            var schema = new Schema
            {
                Name = "Addresses",
                Fields = new List<Field>
                {
                    new Field("id", "string", false),
                    new Field("roadName", "string", false),
                    new Field("houseNumber", "string", false),
                    new Field("townName", "string", false),
                    new Field("postDistrictCode", "string", false),
                    new Field("postDistrictName", "string", false),
                    new Field("eastCoordinate", "string", false),
                    new Field("northCoordinate", "string", false),
                },
            };

            await _typesenseClient.CreateCollection(schema);
        }
    }
}
