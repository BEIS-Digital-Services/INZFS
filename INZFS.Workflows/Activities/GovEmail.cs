using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
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
        private readonly string apiKey = Environment.GetEnvironmentVariable("GovNotifyApiKey");

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

        public override LocalizedString DisplayText => T["Template Gov.Notify Email Task"];
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

        public WorkflowExpression<string> TemplateName
        {
            get => GetProperty(() => new WorkflowExpression<string>());
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
            var recipientsTask = _expressionEvaluator.EvaluateAsync(Recipients, workflowContext, null);
            var templateTask = _expressionEvaluator.EvaluateAsync(TemplateName, workflowContext, null);
            await Task.WhenAll(recipientsTask);
            var client = new NotificationClient(apiKey);
            try
            {
                client.SendEmail(
                                    recipientsTask.Result,
                                    templateTask.Result);
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