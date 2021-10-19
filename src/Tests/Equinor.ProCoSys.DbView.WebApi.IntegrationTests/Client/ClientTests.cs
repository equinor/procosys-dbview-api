using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Client
{
    [TestClass]
    public class ClientTests : ClientSetup
    {
        [TestMethod]
        public void ShouldCreateNotAuthenticatedClient()
        {
            Assert.IsFalse(NotAuthenticatedRestClient.IsAuthenticated);
            Assert.IsNull(NotAuthenticatedRestClient.ClientId);
        }

        [TestMethod]
        public void ShouldCreateAuthenticatedClient()
        {
            Assert.IsTrue(AuthenticatedRestClient.IsAuthenticated);
            Assert.IsNotNull(AuthenticatedRestClient.ClientId);
        }
    }
}
