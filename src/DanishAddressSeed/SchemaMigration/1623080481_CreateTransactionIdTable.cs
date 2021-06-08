using FluentMigrator;

namespace DanishAddressSeed.SchemaMigration
{
    [Migration(1623080482)]
    public class CreateTransactionIdTable : Migration
    {
        public override void Up()
        {
            Create
                .Table("transaction_history")
                .InSchema("location")
                .WithColumn("id").AsInt32().Identity().PrimaryKey()
                .WithColumn("transaction_id").AsString(255).NotNullable()
                .WithColumn("transaction_timestamp").AsCustom("timestamptz").NotNullable();
        }

        public override void Down()
        {
            Delete.Table("transaction_history");
        }
    }
}
