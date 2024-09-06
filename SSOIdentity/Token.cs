using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SSO.DataModels;

namespace SSOIdentity
{
    public class Token : IToken
    {
        private readonly MyAppSettings _myAppSettings;
        AesExtension aesExtension;

        public Token(MyAppSettings myAppSettings)
        {
            _myAppSettings = myAppSettings;
            aesExtension = new AesExtension(_myAppSettings.AESKey, _myAppSettings.AESIV);
        }

        public string GetTokenizedData(UserDataModel user)
        {
            var data = "{UserName:\"" + user.UserName + "\"}";
            var token = aesExtension.EncryptToBase64String(data);
            return token;
        }

        public UserDataModel ReadTokenizedData(string token)
        {
            var decryptedToken = aesExtension.DecryptFromBase64String(token);
            var data = (JObject)JsonConvert.DeserializeObject(decryptedToken);

            var user = new UserDataModel
            {
                UserName = data["UserName"].ToString()
            };

            return user;
        }
    }
}
