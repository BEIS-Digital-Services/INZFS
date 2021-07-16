using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using INZFS.Theme.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace INZFS.Theme.Migrations
{
    public class UserTwoFactorsSettingsIndexMigration : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<UserTwoFactorsSettingsIndex>(table => table
                .Column<string>("UserId", c => c.WithLength(26))
            );

            SchemaBuilder.AlterIndexTable<UserTwoFactorsSettingsIndex>(table => table
                .CreateIndex("IDX_UserTwoFactorsSettingsIndex_UserId",
                    "UserId")
            );

            return 1;
        }
    }
}
