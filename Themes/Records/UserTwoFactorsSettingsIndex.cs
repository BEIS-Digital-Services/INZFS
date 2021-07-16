using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSql.Indexes;

namespace INZFS.Theme.Records
{
    public class UserTwoFactorsSettingsIndex : MapIndex
    {
        public string UserId { get; set; }
    }
}
