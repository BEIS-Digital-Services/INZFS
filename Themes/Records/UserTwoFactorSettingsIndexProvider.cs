using INZFS.Theme.Models;
using YesSql.Indexes;

namespace INZFS.Theme.Records
{
    public class UserTwoFactorSettingsIndexProvider : IndexProvider<UserTwoFactorSettings>
    {
        public override void Describe(DescribeContext<UserTwoFactorSettings> context)
        {
            context.For<UserTwoFactorSettingsIndex>()
                .Map(contentItem =>
                {
                    var contentItemIndex = new UserTwoFactorSettingsIndex
                    {
                        UserId = contentItem.UserId
                    };
                    return contentItemIndex;
                });
        }
    }
}