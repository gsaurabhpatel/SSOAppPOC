using Newtonsoft.Json;
using System;
using System.Security.Claims;
using System.Security.Principal;

namespace WebApp1.Helper
{
    public class UserIdentity
    {
        public string UserId { get; set; }
        public string GivenName { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }

    [Serializable]
    public class CustomIdentity : IIdentity
    {
        private readonly UserIdentity userIdentity;
        private readonly bool isAuthenticated;

        public CustomIdentity(ClaimsIdentity claimsIdentity)
        {
            isAuthenticated = claimsIdentity.IsAuthenticated;
            Claim claim = claimsIdentity?.FindFirst("UserIdentity");
            userIdentity = JsonConvert.DeserializeObject<UserIdentity>(claim.Value);
        }

        public string AuthenticationType
        {
            get { return "CustomIdentity"; }
        }

        public bool IsAuthenticated
        {
            get { return isAuthenticated; }
        }

        public long UserId
        {
            get { return Convert.ToInt64(userIdentity.UserId); }
        }

        public string GivenName
        {
            get { return userIdentity.GivenName; }
        }

        public string Name
        {
            get { return userIdentity.Name; }
        }

        public string Password
        {
            get { return userIdentity.Password; }
        }
    }
}
