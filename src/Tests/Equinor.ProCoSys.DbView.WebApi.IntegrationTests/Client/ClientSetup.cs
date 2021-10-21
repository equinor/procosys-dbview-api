namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Client
{
    public class ClientSetup
    {
        public ClientSetup()
        {
            NotAuthenticatedRestClient = new RestClient(1);
            AuthenticatedRestClient = RestClient.GetAuthenticatedClient().Result;
        }

        protected RestClient NotAuthenticatedRestClient;
        protected RestClient AuthenticatedRestClient;
    }
}
