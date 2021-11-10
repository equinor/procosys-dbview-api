using System;
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
            string cutoffDate,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var requiredParameters = new ParameterCollection
            {
                { "cutoffDate", cutoffDate }
            };
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
            string cutoffDate,
            int currentPage,
            int itemsPerPage,
            HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var requiredParameters = new ParameterCollection
            {
                { "cutoffDate", cutoffDate },
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

        public static string CreateDateOffsetToday(int days)
        {
            var now = DateTime.Now.AddDays(days);
            return now.ToString("yyyy-MM-dd");
        }
    }
}
