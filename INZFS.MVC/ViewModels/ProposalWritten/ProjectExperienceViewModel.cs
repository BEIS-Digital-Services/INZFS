using INZFS.MVC.Models.ProposalWritten;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INZFS.MVC.ViewModels.ProposalWritten
{
    public class ProjectExperienceViewModel : ProjectExperiencePart
    {
        [BindNever]
        public ProjectExperiencePart ProjectExperiencePart { get; set; }
    }
}
