using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace INZFS.Workflows.ViewModels
{
    public class GovEmailViewModel
    {
        public string SenderExpression { get; set; }

        [Required]
        public string RecipientsExpression { get; set; }
        public string SubjectExpression { get; set; }
        public string Body { get; set; }
        public bool IsBodyHtml { get; set; }
        public string TemplateName { get; set; }
    }
}