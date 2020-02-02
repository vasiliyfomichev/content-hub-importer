using Stylelabs.M.Sdk.WebClient;
using System;

namespace ContentHub.Importer.Utils
{
    public static class MConnector
    {
        private static Lazy<IWebMClient> _client { get; set; }

        public static IWebMClient Client
        {
            get
            {
                if (_client == null)
                {
                    var auth = new Stylelabs.M.Sdk.WebClient.Authentication.OAuthPasswordGrant()
                    {
                        ClientId = AppSettings.ClientId,
                        ClientSecret = AppSettings.ClientSecret,
                        UserName = AppSettings.Username,
                        Password = AppSettings.Password
                    };

                    _client = new Lazy<IWebMClient>(() => MClientFactory.CreateMClient(AppSettings.Host, auth));
                    IWebMClient client = MClientFactory.CreateMClient(AppSettings.Host, auth);
                    client.TestConnectionAsync().Wait();
                }

                return _client.Value;
            }
        }
    }
}
