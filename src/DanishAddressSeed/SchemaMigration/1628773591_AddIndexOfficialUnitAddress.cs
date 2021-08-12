
using FluentMigrator;

namespace DanishAddressSeed.SchemaMigration
{
    [Migration(1628773591)]
    public class AddIndexOfficialUnitAddress : Migration
    {
        public override void Up()
        {
            Create.Index()
                .OnTable("official_unit_address")
                .InSchema("location")
                .OnColumn("access_address_id")
                .Unique();

            Create.Index()
                .OnTable("official_unit_address")
                .InSchema("location")
                .OnColumn("access_address_external_id")
                .Unique();
        }

        public override void Down()
        {
            Delete.Index()
                .OnTable("official_unit_address")
                .InSchema("location")
                .OnColumn("access_address_id");

            Delete.Index()
                .OnTable("official_unit_address")
                .InSchema("location")
                .OnColumn("access_address_external_id");
        }
    }
}
