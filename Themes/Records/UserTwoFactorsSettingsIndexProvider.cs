using INZFS.Theme.Models;
using YesSql.Indexes;

namespace INZFS.Theme.Records
{
    public class UserTwoFactorsSettingsIndexProvider : IndexProvider<UserTwoFactorsSettings>
    {
        public override void Describe(DescribeContext<UserTwoFactorsSettings> context)
        {
            context.For<UserTwoFactorsSettingsIndex>()
                .Map(contentItem =>
                {
                    var contentItemIndex = new UserTwoFactorsSettingsIndex
                    {
                        UserId = contentItem.UserId
                    };
                    return contentItemIndex;
                });
        }
    }
}