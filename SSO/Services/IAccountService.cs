using SSO.DataModels;
using SSO.ViewModels;

namespace SSO.Services
{
    public interface IAccountService
    {
        UserDataModel UserLogin(LoginViewModel loginViewModel);
        UserDataModel FindUserByUserName(string userName);
    }
}
