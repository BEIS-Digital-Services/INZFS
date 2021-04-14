using INZFS.MVC.Drivers;
using INZFS.MVC.Models.ProposalWritten;
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
    public class ApplicationDocumentMigration : DataMigration
    {
        public readonly IContentDefinitionManager _contentDefinitionManager;
        public ApplicationDocumentMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("ApplicationDocument", type => type
               .Creatable()
               .Listable()
               .WithPart(nameof(ApplicationDocumentPart))
           );

            return 1;
        }
    }
}
