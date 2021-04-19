using INZFS.MVC.Models.ProposalFinance;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.Migrations.ProposalFinance
{
    public class FinanceBalanceSheetMigration : DataMigration
    {
        public readonly IContentDefinitionManager _contentDefinitionManager;
        public FinanceBalanceSheetMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("FinanceBalanceSheet", type => type
               .Creatable()
               .Listable()
               .WithPart(nameof(FinanceBalanceSheetPart))
           );

            return 1;
        }
    }
}
