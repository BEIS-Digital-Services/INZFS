using INZFS.Workflows.Activities;
using INZFS.Workflows.ViewModels;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;

namespace INZFS.Workflows.Drivers
{
    public class GovEmailDriver : ActivityDisplayDriver<GovEmail, GovEmailViewModel>
    {
        #region Overrides

        protected override void EditActivity(GovEmail activity, GovEmailViewModel model)
        {
            model.SenderExpression = activity.Sender.Expression;
            model.RecipientsExpression = activity.Recipients.Expression;
            model.SubjectExpression = activity.Subject.Expression;
            model.Body = activity.Body.Expression;
            model.IsBodyHtml = activity.IsBodyHtml;
            model.TemplateName = activity.TemplateName.Expression;
        }

        protected override void UpdateActivity(GovEmailViewModel model, GovEmail activity)
        {
            activity.Sender = new WorkflowExpression<string>(model.SenderExpression);
            activity.Recipients = new WorkflowExpression<string>(model.RecipientsExpression);
            activity.Subject = new WorkflowExpression<string>(model.SubjectExpression);
            activity.Body = new WorkflowExpression<string>(model.Body);
            activity.IsBodyHtml = model.IsBodyHtml;
            activity.TemplateName = new WorkflowExpression<string>(model.TemplateName);
        }

        #endregion
    }
}