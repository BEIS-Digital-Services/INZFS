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
    public class ProjectSummaryMigration : DataMigration
    {
        public readonly IContentDefinitionManager _contentDefinitionManager;
        public ProjectSummaryMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition(nameof(ProjectSummaryPart), type => type
               .Creatable()
               .Listable()
               .WithPart(nameof(ProjectSummaryPart))
           );

            return 1;
        }
    }
}
