using System.ComponentModel.DataAnnotations;

namespace SSO.ViewModels
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public bool IsRememberMe { get; set; }
        public string IPAddress { get; set; }
        public bool IsPostBack { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }
}
