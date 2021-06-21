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
    public class ProjectDetailsMigration : DataMigration
    {
        public readonly IContentDefinitionManager _contentDefinitionManager;
        public ProjectDetailsMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("Project Details", type => type
               .Creatable()
               .Listable()
               .WithPart(nameof(ProjectDetailsPart))
           );

            return 1;
        }
    }
}
