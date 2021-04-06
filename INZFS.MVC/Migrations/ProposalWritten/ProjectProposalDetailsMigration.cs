using INZFS.MVC.Models.ProposalWritten;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Migrations.ProposalWritten
{
    public class ProjectProposalDetailsMigration : DataMigration
    {
        public readonly IContentDefinitionManager _contentDefinitionManager;
        public ProjectProposalDetailsMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("ProjectProposalDetails", type => type
               .Creatable()
               .Listable()
               .WithPart(nameof(ProjectProposalDetailsPart))
           );

            return 1;
        }
    }
}
