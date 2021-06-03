using System.IO;
using FluentMigrator;

namespace DanishAddressSeed.SchemaMigration
{
    [Migration(1622715713)]
    public class InitialLocationSchemaSetup : Migration
    {
        public override void Up()
        {
            Execute.Script(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
                           + "/SchemaMigration/Scripts/create_location_schema.sql");
        }

        public override void Down()
        {
            Delete.Schema("location");
        }
    }
}
