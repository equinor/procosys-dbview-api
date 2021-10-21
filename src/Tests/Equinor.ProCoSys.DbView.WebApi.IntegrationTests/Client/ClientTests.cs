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
        public void ShouldCreateAuthenticatedClientWithAccess()
        {
            Assert.IsTrue(ClientWithAccess.IsAuthenticated);
            Assert.IsNotNull(ClientWithAccess.ClientId);
        }

        [TestMethod]
        public void ShouldCreateAuthenticatedClientWithoutAccess()
        {
            Assert.IsTrue(ClientWithoutAccess.IsAuthenticated);
            Assert.IsNotNull(ClientWithoutAccess.ClientId);
        }
    }
}
