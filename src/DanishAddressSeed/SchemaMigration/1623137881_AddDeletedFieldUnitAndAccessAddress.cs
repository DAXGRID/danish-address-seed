using FluentMigrator;

namespace DanishAddressSeed.SchemaMigration
{
    [Migration(1623137881)]
    public class AddDeletedFieldUnitAndAccessAddress : Migration
    {
        public override void Up()
        {
            Create.Column("deleted")
                .OnTable("official_access_address")
                .InSchema("location")
                .AsBoolean()
                .WithDefaultValue(false);

            Create.Column("deleted")
                .OnTable("official_unit_address")
                .InSchema("location")
                .AsBoolean()
                .WithDefaultValue(false);
        }

        public override void Down()
        {
            Delete.Column("deleted")
                .FromTable("official_access_address")
                .InSchema("location");

            Delete.Column("deleted")
                .FromTable("official_unit_address")
                .InSchema("location");
        }
    }
}
