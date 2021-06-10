using INZFS.MVC.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Migrations
{
    public class ApplicationStatusMigration : DataMigration
    {
        public readonly IContentDefinitionManager _contentDefinitionManager;
        public ApplicationStatusMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("ApplicationStatus", type => type
               .Creatable()
               .Listable()
               .WithPart(nameof(ApplicationStatusPart))
           );

            return 1;
        }
    }
}