using INZFS.Theme.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace INZFS.Theme.Migrations
{
    public class UserTwoFactorSettingsIndexMigration : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<UserTwoFactorSettingsIndex>(table => table
                .Column<string>("UserId", c => c.WithLength(100))
            );

            SchemaBuilder.AlterIndexTable<UserTwoFactorSettingsIndex>(table => table
                .CreateIndex("IDX_UserTwoFactorsSettingsIndex_UserId",
                    "UserId")
            );

            return 1;
        }
    }
}
