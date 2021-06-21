﻿using INZFS.MVC.Models.ProposalWritten;
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
    public class ProjectExperienceMigration : DataMigration
    {
        public readonly IContentDefinitionManager _contentDefinitionManager;
        public ProjectExperienceMigration(IContentDefinitionManager contentDefinitionManager) => _contentDefinitionManager = contentDefinitionManager;

        public int Create()
        {
            _contentDefinitionManager.AlterTypeDefinition("ProjectExperience", type => type
               .Creatable()
               .Listable()
               .WithPart(nameof(ProjectExperiencePart))
           );

            return 1;
        }
    }
}
