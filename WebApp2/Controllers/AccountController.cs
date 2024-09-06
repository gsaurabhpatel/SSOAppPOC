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
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp2.Helper;

namespace WebApp2.Controllers
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
                    await IdentityLogIn(user, loginViewModel.IsRememberMe);
                    return RedirectToLocal(returnUrl);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, user.Message);
                    loginViewModel.Message = user.Message;
                }
            }
            loginViewModel.IsPostBack = true;
            return View(loginViewModel);
        }

        public async Task<IActionResult> SSO_Login(string token)
        {
            string error = string.Empty;
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    await IdentityLogOut();
                }

                var sSOUserData = _token.ReadTokenizedData(token);
                if (sSOUserData != null && !string.IsNullOrEmpty(sSOUserData.UserName))
                {
                    var user = _accountService.FindUserByUserName(sSOUserData.UserName);
                    if (user != null && user.UserId != null && user.UserId > 0)
                    {
                        await IdentityLogIn(user, false);
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        error = $"Authorization failed for user {user.UserName}. {Environment.NewLine} Please connect to your service provider.";
                        return RedirectToAction("Login", "Account", new { sso_error = error, token = token });
                    }
                }
            }
            catch (Exception ex)
            {
                error = $"Something went wrong, please try after some time. {Environment.NewLine} Exception: {ex.Message}";
                _logger.LogError(ex.Message);
            }
            return RedirectToAction("Login", "Account", new { sso_error = error, token = token });
        }

        public async Task<IActionResult> Logout()
        {
            await IdentityLogOut();
            return RedirectToAction("Login", "Account");
        }

        private async Task IdentityLogIn(UserDataModel user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", Convert.ToString(user.UserId)),
                new Claim(ClaimTypes.GivenName, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            claims.Add(new Claim("WebApp2UserIdentity", JsonConvert.SerializeObject(user)));

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

        private async Task IdentityLogOut()
        {
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTime.Now.AddDays(-1)
            };

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, authProperties);
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
