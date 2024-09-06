using System.Security.Claims;
using System.Security.Principal;

namespace WebApp1.Helper
{
    public static class UserIdentityExtensions
    {
        public static CustomIdentity CustomIdentity(this IIdentity identity)
        {
            ClaimsIdentity claimsIdentity = identity as ClaimsIdentity;
            return new CustomIdentity(claimsIdentity);
        }
    }
}
