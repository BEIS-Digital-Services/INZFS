using System.Linq;
using INZFS.Templates.Fields;
using INZFS.Templates.Settings;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;

namespace INZFS.Templates
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        // This migration does not need to run on new installations, but because there is no
        // initial migration record, there is no way to shortcut the Create migration.
        public int Create()
        {
            

          
            // Text field
            _contentDefinitionManager.MigrateFieldSettings<GovTextField, GovTextFieldHeaderDisplaySettings>();
            _contentDefinitionManager.MigrateFieldSettings<GovTextField, GovTextFieldPredefinedListEditorSettings>();
            _contentDefinitionManager.MigrateFieldSettings<GovTextField, GovTextFieldSettings>();


            // Shortcut other migration steps on new content definition schemas.
            return 2;
        }

    }
}