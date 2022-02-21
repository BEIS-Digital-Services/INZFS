using INZFS.MVC.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace INZFS.MVC.Migrations
{
    public class ApplicationOutcomeStatusIdIndexMigration : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<ApplicationOutcomeStatusIdIndex>(table => table
                .Column<string>("ApplicationId", c => c.WithLength(100))
            );

            SchemaBuilder.AlterIndexTable<ApplicationOutcomeStatusIdIndex>(table => table
                .CreateIndex("IDX_ApplicationOutcomeStatusIdIndex_ApplicationId",
                    "ApplicationId")
            );

            return 1;
        }
    }
}