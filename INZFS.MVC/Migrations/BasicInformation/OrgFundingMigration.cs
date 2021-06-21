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
    public class OrgFundingMigration : DataMigration
    {
        public readonly IContentDefinitionManager _contentDefinitionManager;
        public OrgFundingMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("Funding", type => type
               .Creatable()
               .Listable()
               .WithPart(nameof(OrgFundingPart))
           );

            return 1;
        }
    }
}
