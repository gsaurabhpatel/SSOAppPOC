using System;

namespace SSO.DataModels
{
    public class UserDataModel
    {
        public long? UserId { get; set; }
        public int? RoleId { get; set; }
        public string Role { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogInDateTime { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
