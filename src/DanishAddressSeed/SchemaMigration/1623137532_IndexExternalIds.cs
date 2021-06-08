using FluentMigrator;

namespace DanishAddressSeed.SchemaMigration
{
    [Migration(1623137532)]
    public class IndexExternalIds : Migration
    {
        public override void Up()
        {
            Create.Index()
                .OnTable("offical_access_address")
                .InSchema("location")
                .OnColumn("access_address_external_id")
                .Unique();

            Create.Index()
                .OnTable("official_unit_address")
                .InSchema("location")
                .OnColumn("unit_address_external_id")
                .Unique();
        }

        public override void Down()
        {
            Delete.Index()
                .OnTable("official_access_address")
                .InSchema("location")
                .OnColumn("access_address_external_id");

            Delete.Index()
                .OnTable("official_unit_address")
                .InSchema("location")
                .OnColumn("unit_address_external_id");
        }
    }
}
