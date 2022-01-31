using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.ThreeDEcoTag
{
    public class TagTestsHelper
    {
        public static async Task<TagModel> GetTagPage(
            RestClient restClient,
            string installationCode,
            int currentPage,
            int itemsPerPage,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var requiredParameters = new ParameterCollection
            {
                { "installationCode", installationCode },
                { "currentPage", currentPage.ToString() },
                { "itemsPerPage", itemsPerPage.ToString() }
            };
            var result = await restClient.Client.GetAsync(Route.DbView.ThreeDEcoTag.Get + requiredParameters);
            Assert.AreEqual(expectedStatusCode, result.StatusCode);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var jsonString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TagModel>(jsonString);
        }
    }
}
