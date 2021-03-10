using INZFS.MVC.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentFields.Settings;
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
    public class PersonMigration : DataMigration
    {
        public readonly IContentDefinitionManager _contentDefinitionManager;
        public PersonMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition(nameof(PersonPart), part => part
                // Each field has its own configuration. Here you will give a display name for it and add some
                // additional settings like a hint to be displayed in the editor.
                .WithField(nameof(PersonPart.Biography), field => field
                    .OfType(nameof(TextField))
                    .WithDisplayName("Biography")
                    .WithEditor("TextArea")
                    .WithSettings(new TextFieldSettings
                    {
                        Hint = "Person's biography",
                    }))
            );

            _contentDefinitionManager.AlterTypeDefinition("PersonPage", type => type
                .Creatable()
                .Listable()
                // We attach parts by specifying their name. For our own parts we use nameof(): This is not mandatory
                // but serves great if we change the part's name during development.
                .WithPart(nameof(PersonPart))
            );

            return 1;
        }
    }
}
