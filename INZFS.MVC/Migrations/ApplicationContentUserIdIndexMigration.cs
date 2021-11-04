using INZFS.MVC.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace INZFS.MVC.Migrations
{
    public class ApplicationContentUserIdIndexMigration : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<ApplicationContentUserIdIndex>(table => table
                .Column<string>("UserId", c => c.WithLength(100))
            );

            SchemaBuilder.AlterIndexTable<ApplicationContentUserIdIndex>(table => table
                .CreateIndex("IDX_ApplicationContentUserIdIndex_UserId",
                    "UserId")
            );

            return 1;
        }
    }
}
