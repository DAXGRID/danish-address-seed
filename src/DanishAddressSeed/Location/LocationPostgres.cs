using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace DanishAddressSeed.Location
{
    internal class LocationPostgres : ILocationPostgres
    {
        private readonly IConfiguration _config;

        public LocationPostgres(IConfiguration config)
        {
            _config = config;
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
    }
}
