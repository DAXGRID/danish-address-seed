using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DanishAddressSeed.Location
{
    internal class LocationPostgres : ILocationPostgres
    {
        private readonly IConfiguration _config;
        private readonly ILogger<LocationPostgres> _logger;

        public LocationPostgres(IConfiguration config, ILogger<LocationPostgres> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task InsertOfficalAccessAddresses(List<OfficalAccessAddress> addresses)
        {
            using var connection = new NpgsqlConnection(_config.GetValue<string>("CONNECTION_STRING"));
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            var query = @"
                INSERT INTO location.official_access_address (
                    id,
                    coord,
                    status,
                    house_number,
                    road_code,
                    road_name,
                    town_name,
                    post_district_code,
                    post_district_name,
                    municipal_code,
                    access_address_external_id,
                    road_external_id,
                    plot_external_id,
                    created,
                    updated,
                    location_updated
                ) VALUES (
                    @id,
                    ST_SetSRID(ST_MakePoint(@east, @north), 25832),
                    @status,
                    @house_number,
                    @road_code,
                    @road_name,
                    @town_name,
                    @post_district_code,
                    @post_district_name,
                    @municipal_code,
                    @access_address_external_id,
                    @road_external_id,
                    @plot_external_id,
                    @created,
                    @updated,
                    @location_updated
                )
            ";

            foreach (var address in addresses)
            {
                using var command = new NpgsqlCommand(query)
                {
                    Connection = connection,
                    Transaction = transaction
                };

                command.Parameters.AddWithValue("@id", Guid.NewGuid());
                command.Parameters.AddWithValue("@east", address.EastCoordinate);
                command.Parameters.AddWithValue("@north", address.NorthCoordinate);
                command.Parameters.AddWithValue("@status", address.Status);
                command.Parameters.AddWithValue(
                    "@house_number", string.IsNullOrEmpty(address.HouseNumber) ? DBNull.Value : address.HouseNumber);
                command.Parameters.AddWithValue(
                    "@road_code", string.IsNullOrEmpty(address.RoadCode) ? DBNull.Value : address.RoadCode);
                command.Parameters.AddWithValue(
                    "@town_name", string.IsNullOrEmpty(address.TownName) ? DBNull.Value : address.TownName);
                command.Parameters.AddWithValue(
                    "@post_district_code",
                    string.IsNullOrEmpty(address.PostDistrictCode) ? DBNull.Value : address.PostDistrictCode);
                command.Parameters.AddWithValue(
                    "@post_district_name",
                    string.IsNullOrEmpty(address.PostDistrictName) ? DBNull.Value : address.PostDistrictName);
                command.Parameters.AddWithValue(
                    "@municipal_code",
                    string.IsNullOrEmpty(address.MunicipalCode) ? DBNull.Value : address.MunicipalCode);
                command.Parameters.AddWithValue(
                    "@access_address_external_id",
                    string.IsNullOrEmpty(address.AccessAdddressExternalId) ? DBNull.Value : address.AccessAdddressExternalId);
                command.Parameters.AddWithValue(
                    "@road_external_id",
                    string.IsNullOrEmpty(address.RoadExternalId) ? DBNull.Value : address.RoadExternalId);
                command.Parameters.AddWithValue(
                    "@plot_external_id",
                    string.IsNullOrEmpty(address.PlotExternalId) ? DBNull.Value : address.PlotExternalId);
                command.Parameters.AddWithValue(
                    "@road_name",
                    string.IsNullOrEmpty(address.RoadName) ? DBNull.Value : address.RoadName);
                command.Parameters.AddWithValue("@created", address.Created);
                command.Parameters.AddWithValue("@updated", address.Updated);
                command.Parameters.AddWithValue("@location_updated", address.LocationUpdated);

                await command.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }

        public async Task InsertOfficialUnitAddresses(List<OfficalUnitAddress> addresses)
        {
            using var connection = new NpgsqlConnection(_config.GetValue<string>("CONNECTION_STRING"));
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            var getAccessAddressIdQuery = @"SELECT id
                         FROM location.official_access_address
                         WHERE access_address_external_id = @access_address_external_id";

            var insertUnitAddressQuery = @"
                INSERT INTO location.official_unit_address (
                        id,
                        access_address_id,
                        status,
                        floor_name,
                        suit_name,
                        unit_address_external_id,
                        created,
                        updated,
                        access_address_external_id
                    ) VALUES (
                        @id,
                        @access_address_id,
                        @status,
                        @floor_name,
                        @suit_name,
                        @unit_address_external_id,
                        @created,
                        @updated,
                        @access_address_external_id
                    );
                ";

            foreach (var address in addresses)
            {
                using var getAccessAddresIdCmd = new NpgsqlCommand(getAccessAddressIdQuery)
                {
                    Connection = connection,
                    Transaction = transaction
                };

                getAccessAddresIdCmd.Parameters.AddWithValue("@access_address_external_id", address.AccessAddressExternalId);

                var result = await getAccessAddresIdCmd.ExecuteScalarAsync();

                if (result is null)
                {
                    _logger.LogWarning(
                        $"Skipping acess address id could not be found on external id: '{address.AccessAddressExternalId}");
                    continue;
                }

                address.AccessAddressId = (Guid)result;

                using var insertUnitAddressCmd = new NpgsqlCommand(insertUnitAddressQuery)
                {
                    Connection = connection,
                    Transaction = transaction
                };

                insertUnitAddressCmd.Parameters.AddWithValue("@id", Guid.NewGuid());
                insertUnitAddressCmd.Parameters.AddWithValue("@access_address_id", address.AccessAddressId);
                insertUnitAddressCmd.Parameters.AddWithValue("@status", address.Status);
                insertUnitAddressCmd.Parameters.AddWithValue(
                    "@floor_name",
                    string.IsNullOrEmpty(address.FloorName) ? DBNull.Value : address.FloorName);
                insertUnitAddressCmd.Parameters.AddWithValue(
                    "@suit_name",
                    string.IsNullOrEmpty(address.SuitName) ? DBNull.Value : address.SuitName);
                insertUnitAddressCmd.Parameters.AddWithValue("@unit_address_external_id", address.UnitAddressExternalId);
                insertUnitAddressCmd.Parameters.AddWithValue("@created", address.Created);
                insertUnitAddressCmd.Parameters.AddWithValue("@updated", address.Updated);
                insertUnitAddressCmd.Parameters.AddWithValue("@access_address_external_id", address.AccessAddressExternalId);

                await insertUnitAddressCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
        }
    }
}
