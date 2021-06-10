using INZFS.MVC.Forms;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Flows.Models;

namespace INZFS.MVC.Migrations
{
    public class ApplicationContainerMigration : DataMigration
    {
        public readonly IContentDefinitionManager _contentDefinitionManager;
        public ApplicationContainerMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition(ContentTypes.INZFSApplicationContainer, type => type
            .Creatable()
            .Listable()
            .WithPart("BagPart", "BagPart", builder => builder
                .WithDisplayName("INZFS Application")
                .WithDescription("INZFSApplication description")
                .MergeSettings<BagPartSettings>(x => x.ContainedContentTypes = new string[] {
                    ContentTypes.CompanyDetails,
                    ContentTypes.ProjectSummary,
                    ContentTypes.ProjectDetails,
                    ContentTypes.OrgFunding,
                    ContentTypes.ProjectProposalDetails,
                    ContentTypes.ApplicationDocument,
                    ContentTypes.ProjectExperience,
                    ContentTypes.FinanceTurnover,
                    ContentTypes.FinanceBalanceSheet,
                    ContentTypes.FinanceRecoverVat,
                    ContentTypes.FinanceBarriers, 
                    ContentTypes.ApplicationStatus }))
        );

            return 1;
        }
    }
}
