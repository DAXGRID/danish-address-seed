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
        private const string CollectionName = "Addresses";

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

            var lastTransactionId = await _locationPostgres.GetLatestTransactionHistory().ConfigureAwait(false);
            var newTransactionId = await _client.GetTransactionId().ConfigureAwait(false);

            if (string.IsNullOrEmpty(lastTransactionId))
            {
                _logger.LogInformation("First time running, doing full bulk import");
                await SetupTypesense().ConfigureAwait(false);
                await BulkInsertAccessAddresses(newTransactionId).ConfigureAwait(false);
                await BulkInsertUnitAddresses(newTransactionId).ConfigureAwait(false);
            }
            else
            {
                _logger.LogInformation($"Getting latest changes using existing tid {lastTransactionId} and new tid {newTransactionId}");
                await UpdateAccessAddresses(lastTransactionId, newTransactionId).ConfigureAwait(false);
                await UpdateUnitAddresses(lastTransactionId, newTransactionId).ConfigureAwait(false);
            }

            _logger.LogInformation($"Insert new transaction history with transaction_id '{newTransactionId}'");
            await _locationPostgres.InsertTransactionHistory(
                newTransactionId, DateTime.UtcNow).ConfigureAwait(false);

            _logger.LogInformation($"Finished {nameof(DanishAddressSeed)}");
        }

        private async Task UpdateAccessAddresses(string fromTransId, string toTransId)
        {
            var count = 0;
            await foreach (var changeEvent in _client.RetrieveChangesOfficalAccessAddress(fromTransId, toTransId).ConfigureAwait(false))
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

                        await _locationPostgres.UpdateOfficalAccessAddress(address).ConfigureAwait(false);
                        await _typesenseClient.UpdateDocument<TypesenseOfficalAccessAddress>
                            (CollectionName, address.Id.ToString(), _locationMapper.Map(address)).ConfigureAwait(false);
                        break;
                    case "insert":
                        await _locationPostgres.InsertOfficalAccessAddresses(
                            new List<OfficialAccessAddress> { address });
                        await _typesenseClient.CreateDocument<TypesenseOfficalAccessAddress>
                            (CollectionName, _locationMapper.Map(address)).ConfigureAwait(false);
                        break;
                    case "delete":
                        id = await _locationPostgres.GetAccessAddressOnExternalId(
                            changeEvent.Data.AccessAdddressExternalId).ConfigureAwait(false);
                        if (Guid.Empty == id)
                            throw new Exception("Id cannot be empty on an access address deletion");

                        // We update the id here to make it match the one in the database
                        address = address with { Id = id };

                        // We do an update here where the deleted is set
                        await _locationPostgres.UpdateOfficalAccessAddress(address).ConfigureAwait(false);
                        // We use the primary id from the database since we don't have it on the address object
                        await _typesenseClient.DeleteDocument<TypesenseOfficalAccessAddress>(
                            CollectionName, address.Id.ToString()).ConfigureAwait(false);
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
            await foreach (var changeEvent in _client.RetrieveChangesOfficialUnitAddress(fromTransId, toTransId).ConfigureAwait(false))
            {
                count++;
                switch (changeEvent.Operation)
                {
                    case "update":
                        await _locationPostgres.UpdateOfficialUnitAddress(changeEvent.Data).ConfigureAwait(false);
                        break;
                    case "insert":
                        await _locationPostgres.InsertOfficialUnitAddresses(
                            new List<OfficialUnitAddress> { changeEvent.Data }).ConfigureAwait(false);
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
            await foreach (var accessAddress in _client.RetrieveAllOfficialAccessAddresses(newTransactionId).ConfigureAwait(false))
            {
                addresses.Add(accessAddress);

                if (addresses.Count == ImportBatchCount)
                {
                    await _locationPostgres.InsertOfficalAccessAddresses(addresses).ConfigureAwait(false);
                    await _typesenseClient.ImportDocuments<TypesenseOfficalAccessAddress>(
                        CollectionName,
                        addresses.Select(x => _locationMapper.Map(x)).ToList(),
                        addresses.Count,
                        ImportType.Create).ConfigureAwait(false);

                    count += addresses.Count;
                    _logger.LogInformation($"Imported: {count}");

                    addresses.Clear();
                }
            }

            await _locationPostgres.InsertOfficalAccessAddresses(addresses).ConfigureAwait(fasle);
            await _typesenseClient.ImportDocuments<TypesenseOfficalAccessAddress>(
                CollectionName,
                addresses.Select(x => _locationMapper.Map(x)).ToList(),
                addresses.Count,
                ImportType.Create).ConfigureAwait(false);
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
                    await _locationPostgres.InsertOfficialUnitAddresses(addresses).ConfigureAwait(false);
                    addresses.Clear();
                }
            }

            await _locationPostgres.InsertOfficialUnitAddresses(addresses).ConfigureAwait(false);
        }

        private async Task SetupTypesense()
        {
            try
            {
                // We do this to make sure that the state is clean on full bulk import
                await _typesenseClient.DeleteCollection(CollectionName).ConfigureAwait(false);
            }
            catch (TypesenseApiNotFoundException)
            {
                // We do not do anything since, we only wanted to delete it, if it existed.
            }

            var schema = new Schema(
                CollectionName,
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

            await _typesenseClient.CreateCollection(schema).ConfigureAwait(false);
        }
    }
}
