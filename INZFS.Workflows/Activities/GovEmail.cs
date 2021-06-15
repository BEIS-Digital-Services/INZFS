using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Email;
using OrchardCore.Liquid;
using OrchardCore.Templates.Services;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Notify.Client;
using System;

namespace INZFS.Workflows.Activities
{
    public class GovEmail : TaskActivity
    {
        private readonly IWorkflowExpressionEvaluator _expressionEvaluator;
        private readonly ILogger<GovEmail> _logger;

        public GovEmail(
            IWorkflowExpressionEvaluator expressionEvaluator,
            ILiquidTemplateManager liquidTemplateManager,
            IStringLocalizer<GovEmail> localizer,
            ILogger<GovEmail> logger
        )
        {
            _expressionEvaluator = expressionEvaluator;
            _logger = logger;
            T = localizer;
        }

        private IStringLocalizer T { get; }

        public override LocalizedString DisplayText => T["Template Email Task"];
        public override string Name => nameof(GovEmail);
        public override LocalizedString Category => T["Messaging"];

        public WorkflowExpression<string> Sender
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        // TODO: Add support for the following format: Jack Bauer<jack@ctu.com>, ...
        public WorkflowExpression<string> Recipients
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Subject
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public Dictionary<string, string> TemplateNames
        {
            get => GetProperty(() => new Dictionary<string, string>());
            set => SetProperty(value);
        }

        public WorkflowExpression<string> Body
        {
            get => GetProperty(() => new WorkflowExpression<string>());
            set => SetProperty(value);
        }

        public bool IsBodyHtml
        {
            get => GetProperty(() => true);
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            return Outcomes(T["Done"], T["Failed"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext, ActivityContext activityContext)
        {
            var apiKey = "lltestapi-bb94d8fd-a2ae-472a-b355-9c39d6d0b916-32fd33b5-e505-4bba-b304-d5cbfd3cdea0";
            var recipientsTask = _expressionEvaluator.EvaluateAsync(Recipients, workflowContext, null);
            
            await Task.WhenAll(recipientsTask);
            var client = new NotificationClient(apiKey);
            try
            {
                client.SendEmail(
                                    emailAddress: "lorenzo.lane@beis.gov.uk",
                                    templateId: "8ca9aa23-ecf9-4f57-b5f3-0d662d5e7237");
                return Outcomes("Succeeded");
            }
            catch (Exception ex)
            {
                Console.WriteLine("The following error has occurred: " + ex);
                return Outcomes("Failed");
            }                
               
        }
    }
}