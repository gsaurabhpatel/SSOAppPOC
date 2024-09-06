using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SSO;
using SSO.DataModels;
using SSO.Services;
using SSO.ViewModels;
using SSOIdentity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using WebApp1.Helper;

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
        public IActionResult Login(LoginViewModel loginViewModel, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var user = _accountService.UserLogin(loginViewModel);
                if (user != null && user.UserId != null && user.UserId > 0)
                {
                    IdentityLogin(user, loginViewModel.IsRememberMe);
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

            string requestUri = $"{_myAppSettings.WebApp2Url}Account/SSO_Login?token={WebUtility.UrlEncode(token)}";

            Response.Redirect(requestUri);
        }

        public IActionResult Logout()
        {
            IdentityLogout();
            return RedirectToAction("Login", "Account");
        }

        private async void IdentityLogin(UserDataModel userDataModel, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", Convert.ToString(userDataModel.UserId)),
                new Claim(ClaimTypes.GivenName, $"{userDataModel.FirstName} {userDataModel.LastName}"),
                new Claim(ClaimTypes.Name, userDataModel.UserName)
            };
            claims.Add(new Claim("WebApp1UserIdentity", JsonConvert.SerializeObject(userDataModel)));

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

        private async void IdentityLogout()
        {
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTime.Now.AddDays(-1)
            };

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
