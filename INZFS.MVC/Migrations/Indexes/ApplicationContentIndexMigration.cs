using System;
using INZFS.MVC.Records;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace INZFS.MVC.Migrations.Indexes
{
    public class ApplicationContentIndexMigration : DataMigration
    {
         public int Create()
        {

            SchemaBuilder.CreateMapIndexTable<ApplicationContentIndex>(table => table
               .Column<string>("ContentItemId", c => c.WithLength(26))
               .Column<string>("ContentItemVersionId", c => c.WithLength(26))
               .Column<bool>("Latest")
               .Column<bool>("Published")
               .Column<string>("ContentType", column => column.WithLength(ApplicationContentIndex.MaxContentTypeSize))
               .Column<DateTime>("ModifiedUtc", column => column.Nullable())
               .Column<DateTime>("PublishedUtc", column => column.Nullable())
               .Column<DateTime>("CreatedUtc", column => column.Nullable())
               .Column<string>("Owner", column => column.Nullable().WithLength(ApplicationContentIndex.MaxOwnerSize))
               .Column<string>("Author", column => column.Nullable().WithLength(ApplicationContentIndex.MaxAuthorSize))
               .Column<string>("DisplayText", column => column.Nullable().WithLength(ApplicationContentIndex.MaxDisplayTextSize))
           );

            //SchemaBuilder.AlterTable(nameof(ApplicationContentIndex), table => table
            //    .CreateIndex($"IDX_{nameof(ApplicationContentIndex)}_{nameof(ApplicationContentIndex.Author)}", nameof(ApplicationContentIndex.Author))
            //);

            SchemaBuilder.AlterIndexTable<ApplicationContentIndex>(table => table
                .CreateIndex("IDX_ApplicationContentIndex_DocumentId",
                    "DocumentId",
                    "ContentItemId",
                    "ContentItemVersionId",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<ApplicationContentIndex>(table => table
                .CreateIndex("IDX_ApplicationContentIndex_DocumentId_ContentType",
                    "DocumentId",
                    "ContentType",
                    "CreatedUtc",
                    "ModifiedUtc",
                    "PublishedUtc",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<ApplicationContentIndex>(table => table
                .CreateIndex("IDX_ApplicationContentIndex_DocumentId_Owner",
                    "DocumentId",
                    "Owner",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<ApplicationContentIndex>(table => table
                .CreateIndex("IDX_ApplicationContentIndex_DocumentId_Author",
                    "DocumentId",
                    "Author",
                    "Published",
                    "Latest")
            );

            SchemaBuilder.AlterIndexTable<ApplicationContentIndex>(table => table
                .CreateIndex("IDX_ApplicationContentIndex_DocumentId_DisplayText",
                    "DocumentId",
                    "DisplayText",
                    "Published",
                    "Latest")
            );

            return 1;
        }


    }
}

