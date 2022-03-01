using YesSql.Indexes;

namespace INZFS.MVC.Records
{
    public class ApplicationOutcomeStatusIdIndexProvider : IndexProvider<ApplicationOutcomeStatus>
    {
        public override void Describe(DescribeContext<ApplicationOutcomeStatus> context)
        {
            context.For<ApplicationOutcomeStatusIdIndex>()
                .Map(contentItem =>
                {
                    var contentItemIndex = new ApplicationOutcomeStatusIdIndex
                    {
                        ApplicationId = contentItem.ApplicationId
                    };

                    return contentItemIndex;
                });
        }
    }
}