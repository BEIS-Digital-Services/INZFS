using INZFS.MVC.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace INZFS.MVC.ViewModels.ProposalWritten
{
    public class ApplicationDocumentViewModel : ApplicationDocumentPart
    {
        [BindNever]
        public ApplicationDocumentPart ApplicationDocumentPart { get; set; }
    }
}
