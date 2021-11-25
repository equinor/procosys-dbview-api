using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Client
{
    [TestClass]
    public class ClientTests : ClientSetup
    {
        [TestCategory("All")]
        [TestMethod]
        public void ShouldCreateNotAuthenticatedClient()
        {
            Assert.IsFalse(NotAuthenticatedRestClient.IsAuthenticated);
            Assert.IsNull(NotAuthenticatedRestClient.ClientId);
        }

        [TestCategory("All")]
        [TestMethod]
        public void ShouldCreateAuthenticatedClientWithAccess()
        {
            Assert.IsTrue(ClientWithAccess.IsAuthenticated);
            Assert.IsNotNull(ClientWithAccess.ClientId);
        }

        [TestCategory("All")]
        [TestMethod]
        public void ShouldCreateAuthenticatedClientWithoutAccess()
        {
            Assert.IsTrue(ClientWithoutAnyRoles.IsAuthenticated);
            Assert.IsNotNull(ClientWithoutAnyRoles.ClientId);
        }
    }
}
