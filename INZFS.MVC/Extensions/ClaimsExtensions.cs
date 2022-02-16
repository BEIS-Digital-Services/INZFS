using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace INZFS.MVC.Extensions
{

    public static class ClaimsExtensions
    {
        public static string ApplicationNumber(this IIdentity identity)
        {
            var claimIdentity = (ClaimsIdentity)identity;
            return claimIdentity.Claims.FirstOrDefault(c => c.Type == "ApplicationNumber")?.Value;
        }
    }
}
