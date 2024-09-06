using SSO.DataModels;

namespace SSOIdentity
{
    public interface IToken
    {
        string GetTokenizedData(UserDataModel user);
        UserDataModel ReadTokenizedData(string token);
    }
}
