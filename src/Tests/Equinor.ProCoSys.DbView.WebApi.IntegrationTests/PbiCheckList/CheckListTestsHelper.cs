using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.PbiCheckList
{
    public class CheckListTestsHelper
    {
        public static async Task<PbiCheckListMaxAvailableModel> GetMaxAvailable(
            RestClient restClient,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var requiredParameters = new ParameterCollection();
            var result = await restClient.Client.GetAsync(Route.DbView.PbiCheckList.Count + requiredParameters);
            Assert.AreEqual(expectedStatusCode, result.StatusCode);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var jsonString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PbiCheckListMaxAvailableModel>(jsonString);
        }

        public static async Task<PbiCheckListModel> GetCheckListPage(
            RestClient restClient,
            int currentPage,
            int itemsPerPage,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var requiredParameters = new ParameterCollection
            {
                { "currentPage", currentPage.ToString() },
                { "itemsPerPage", itemsPerPage.ToString() }
            };
            var result = await restClient.Client.GetAsync(Route.DbView.PbiCheckList.Get + requiredParameters);
            Assert.AreEqual(expectedStatusCode, result.StatusCode);

            if (result.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var jsonString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PbiCheckListModel>(jsonString);
        }
    }
}
