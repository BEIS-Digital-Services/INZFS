using INZFS.MVC.Models.Application;
using YesSql.Indexes;

namespace INZFS.MVC.Records
{
    public class ApplicationContentUserIdIndexProvider : IndexProvider<ApplicationContent>
    {
        public override void Describe(DescribeContext<ApplicationContent> context)
        {
            context.For<ApplicationContentUserIdIndex>()
                .Map(contentItem =>
                {
                    var contentItemIndex = new ApplicationContentUserIdIndex
                    {
                        UserId = contentItem.UserId
                    };
                    return contentItemIndex;
                });
        }
    }
}