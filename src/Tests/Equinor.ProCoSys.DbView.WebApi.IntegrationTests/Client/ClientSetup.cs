namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Client
{
    public class ClientSetup
    {
        public ClientSetup()
        {
            NotAuthenticatedRestClient = new RestClient(1);
            ClientWithAccess = RestClient.GetAuthenticatedClient("TestClient1").Result;
            ClientWithoutAccess = RestClient.GetAuthenticatedClient("TestClient2").Result;
        }

        protected RestClient NotAuthenticatedRestClient;
        protected RestClient ClientWithAccess;
        protected RestClient ClientWithoutAccess;
    }
}
