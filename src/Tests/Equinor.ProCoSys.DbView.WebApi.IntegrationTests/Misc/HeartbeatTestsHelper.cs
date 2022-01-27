using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Misc
{
    public class HeartbeatTestsHelper
    {
        public static async Task<HeartbeatModel> GetHeartbeatAsync(
            RestClient restClient,
            bool checkDbIsAlive,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var requiredParameters = new ParameterCollection
            {
                { "checkDbIsAlive", checkDbIsAlive.ToString().ToLower() }
            };
            var result = await restClient.Client.GetAsync(Route.Heartbeat.Get + requiredParameters);
            Assert.AreEqual(expectedStatusCode, result.StatusCode);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var jsonString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<HeartbeatModel>(jsonString);
        }
    }
}
