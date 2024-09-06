using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO.DataModels;
using SSO.Services;
using SSO.ViewModels;
using SSOIdentity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApp1.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyAppSettings _myAppSettings;
        private readonly IToken _token;
        private readonly IAccountService _accountService;

        public AccountController(ILogger<HomeController> logger, MyAppSettings myAppSettings, IToken token, IAccountService accountService)
        {
            _logger = logger;
            _myAppSettings = myAppSettings;
            _token = token;
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(new LoginViewModel());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = _accountService.UserLogin(loginViewModel);
                if (user != null && user.UserId != null && user.UserId > 0)
                {
                    await IdentityLogin(user, loginViewModel.IsRememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, user.Message);
                    loginViewModel.Message = user.Message;
                }
            }
            return View(loginViewModel);
        }

        public void SSOLogin()
        {
            var user = new UserDataModel
            {
                UserName = User.Identity.Name,
            };

            var token = _token.GetTokenizedData(user);
            if (!string.IsNullOrEmpty(token))
            {
                string requestUri = $"{_myAppSettings.WebApp2Url}Account/SSO_Login?token={WebUtility.UrlEncode(token)}";
                Response.Redirect(requestUri);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await IdentityLogout();
            return RedirectToAction("Login", "Account");
        }

        private async Task IdentityLogin(UserDataModel user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", Convert.ToString(user.UserId)),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            claims.Add(new Claim("App1UserIdentity", JsonConvert.SerializeObject(user)));

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddYears(_myAppSettings.PersistentCookieExpireTimeSpan) :
                DateTimeOffset.UtcNow.AddMinutes(_myAppSettings.CookieExpireTimeSpan),
                IsPersistent = isPersistent
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity), authProperties);
        }

        private async Task IdentityLogout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
