using System.Threading.Tasks;
using Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Misc
{
    [TestClass]
    public class HeartbeatTests : ClientSetup
    {
        [TestCategory("All")]
        [TestMethod]
        public async Task A1_ShouldReturnHeartbeatIfNotAuthenticated()
        {
            var model = await HeartbeatTestsHelper.GetHeartbeat(NotAuthenticatedRestClient);
            Assert.IsTrue(model.IsAlive);
        }

        [TestCategory("All")]
        [TestMethod]
        public async Task A2_ShouldReturnHeartbeatWhenClientHasNoRoles()
        {
            var model = await HeartbeatTestsHelper.GetHeartbeat(ClientWithoutAnyRoles);
            Assert.IsTrue(model.IsAlive);
        }
    }
}
