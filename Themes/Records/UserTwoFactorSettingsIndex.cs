using YesSql.Indexes;

namespace INZFS.Theme.Records
{
    public class UserTwoFactorSettingsIndex : MapIndex
    {
        public string UserId { get; set; }
    }
}
