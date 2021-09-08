using INZFS.Theme.Records;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace INZFS.Theme.Migrations
{
    public class RegistrationQuestionnaireIndexMigration : DataMigration
    {
        public int Create()
        {
            SchemaBuilder.CreateMapIndexTable<RegistrationQuestionnaireIndex>(table => table
                .Column<string>("UserId", c => c.WithLength(100))
            );

            SchemaBuilder.AlterIndexTable<RegistrationQuestionnaireIndex>(table => table
                .CreateIndex("IDX_RegistrationQuestionnaireIndex_UserId",
                    "UserId")
            );

            return 1;
        }
    }
}