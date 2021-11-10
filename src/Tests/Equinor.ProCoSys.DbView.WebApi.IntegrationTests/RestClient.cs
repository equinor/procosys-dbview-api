using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests
{
    public class RestClient
    {
        internal HttpClient Client;
        internal bool IsAuthenticated;
        internal string ClientId;

        public RestClient(int timeoutMinutes)
        {
            Client = new HttpClient
            {
                BaseAddress = new Uri(Config.ApplicationUrl),
                Timeout = new TimeSpan(0, timeoutMinutes, 0)
            };
            Console.WriteLine($@"Testing against {Client.BaseAddress.AbsoluteUri}");
        }

        public static async Task<RestClient> GetAuthenticatedClient(string clientConfigKey)
        {
            // big timeout for Admin client because testing slow endpoint
            var restClient = new RestClient(15);
            await restClient.AuthenticateAsync(clientConfigKey);
            return restClient;
        }

        private async Task AuthenticateAsync(string clientConfigKey)
        {
            ClientId = Config.TestClientId(clientConfigKey);

            Console.WriteLine($@"Authenticating against {Config.Authority} as {ClientId}");
            var clientSecret = Config.TestClientSecret(clientConfigKey);
            Console.WriteLine($@"Using secret {clientSecret.Substring(0,4)}... ");
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(ClientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri(Config.Authority))
                .Build();

            var scopes = new[] { Config.WebApiScope };

            var result = await confidentialClientApplication
                    .AcquireTokenForClient(scopes)
                    .ExecuteAsync();

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", result.AccessToken);
            IsAuthenticated = true;
        }
    }
}
