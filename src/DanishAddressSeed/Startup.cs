using DanishAddressSeed.Dawa;
using DanishAddressSeed.Location;
using DanishAddressSeed.Mapper;
using FluentMigrator.Runner;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ILocationMapper _locationMapper;
        private const Int16 ImportBatchCount = 500;

        public Startup(
            ILogger<Startup> logger,
            IMigrationRunner migrationRunner,
            IDawaClient client,
            ILocationPostgres locationPostgres = null,
            ITypesenseClient typesenseClient = null,
            ILocationMapper locationMapper = null)
        {
            _logger = logger;
            _migrationRunner = migrationRunner;
            _client = client;
            _locationPostgres = locationPostgres;
            _typesenseClient = typesenseClient;
            _locationMapper = locationMapper;
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
                var address = changeEvent.Data;
                switch (changeEvent.Operation)
                {
                    case "update":
                        var id = await _locationPostgres.GetAccessAddressOnExternalId(changeEvent.Data.AccessAdddressExternalId);
                        if (Guid.Empty == id)
                            throw new Exception("Id cannot be empty on an access address update");

                        // We update the id here to make it match the one in the database
                        address = address with { Id = id };

                        await _locationPostgres.UpdateOfficalAccessAddress(address);
                        await _typesenseClient.UpdateDocument<TypesenseOfficalAccessAddress>
                            ("Addresses", address.Id.ToString(), _locationMapper.Map(address));
                        break;
                    case "insert":
                        await _locationPostgres.InsertOfficalAccessAddresses(
                            new List<OfficialAccessAddress> { address });
                        await _typesenseClient.CreateDocument<TypesenseOfficalAccessAddress>
                            ("Addresses", _locationMapper.Map(address));
                        break;
                    case "delete":
                        id = await _locationPostgres.GetAccessAddressOnExternalId(changeEvent.Data.AccessAdddressExternalId);
                        if (Guid.Empty == id)
                            throw new Exception("Id cannot be empty on an access address deletion");

                        // We update the id here to make it match the one in the database
                        address = address with { Id = id };

                        // We do an update here where the deleted is set
                        await _locationPostgres.UpdateOfficalAccessAddress(address);
                        // We use the primary id from the database since we don't have it on the address object
                        await _typesenseClient.DeleteDocument<TypesenseOfficalAccessAddress>("Addresses", address.Id.ToString());
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
                        // We do an update here where the deleted is set
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
                    await _locationPostgres.InsertOfficalAccessAddresses(addresses);
                    await _typesenseClient.ImportDocuments<TypesenseOfficalAccessAddress>(
                        "Addresses",
                        addresses.Select(x => _locationMapper.Map(x)).ToList(),
                        addresses.Count,
                        ImportType.Create);

                    count += addresses.Count;
                    _logger.LogInformation($"Imported: {count}");

                    addresses.Clear();
                }
            }

            await _locationPostgres.InsertOfficalAccessAddresses(addresses);
            await _typesenseClient.ImportDocuments<TypesenseOfficalAccessAddress>(
                "Addresses",
                addresses.Select(x => _locationMapper.Map(x)).ToList(),
                addresses.Count,
                ImportType.Create);
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
                    await _locationPostgres.InsertOfficialUnitAddresses(addresses);
                    addresses.Clear();
                }
            }

            await _locationPostgres.InsertOfficialUnitAddresses(addresses);
        }

        private async Task SetupTypesense()
        {
            var collections = await _typesenseClient.RetrieveCollections();
            // We do this to make sure that the state is clean on full bulk import
            foreach (var collection in collections)
            {
                await _typesenseClient.DeleteCollection(collection.Name);
            }

            var schema = new Schema(
                "Addresses",
                new List<Field>
                {
                    new Field("id", FieldType.String, false),
                    new Field("roadNameHouseNumber", FieldType.String, false),
                    new Field("townName", FieldType.String, false, true),
                    new Field("postDistrictCode", FieldType.String, false, true),
                    new Field("postDistrictName", FieldType.String, false, true),
                    new Field("eastCoordinate", FieldType.String, false),
                    new Field("northCoordinate", FieldType.String, false),
                });

            await _typesenseClient.CreateCollection(schema);
        }
    }
}
