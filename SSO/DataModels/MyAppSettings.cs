namespace SSO.DataModels
{
    public class MyAppSettings
    {
        public string SelfRootUrl { get; set; }
        public double CookieExpireTimeSpan { get; set; }
        public int PersistentCookieExpireTimeSpan { get; set; }
        public string AESKey { get; set; }
        public string AESIV { get; set; }
        public string WebApp2Url { get; set; }
    }
}
