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

        public static async Task<RestClient> GetAuthenticatedClient()
        {
            // big timeout for Admin client because testing slow endpoint
            var restClient = new RestClient(15);
            await restClient.AuthenticateAsync();
            return restClient;
        }

        private async Task AuthenticateAsync()
        {
            ClientId = Config.ClientId;

            Console.WriteLine($@"Authenticating against {Config.Authority} as {ClientId}");
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(ClientId)
                .WithClientSecret(Config.ClientSecret)
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
