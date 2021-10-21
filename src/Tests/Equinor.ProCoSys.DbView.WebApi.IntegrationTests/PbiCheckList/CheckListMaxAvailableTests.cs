using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.PbiCheckList
{
    [TestClass]
    public class CheckListMaxAvailableTests : ClientSetup
    {
        [TestMethod]
        public async Task A1_ShouldReturnUnauthorizedIfNotAuthenticated()
            => await CheckListTestsHelper.GetMaxAvailable(NotAuthenticatedRestClient, HttpStatusCode.Unauthorized);

        [TestMethod]
        public async Task A2_ShouldReturnForbiddenIfNoAccess()
            => await CheckListTestsHelper.GetMaxAvailable(ClientWithoutAccess, HttpStatusCode.Forbidden);

        //[Ignore("Very long running test. To be used during development at localhost")]
        [TestMethod]
        public async Task B_ShouldGetMaxAvailableIfHasAccess()
        {
            PbiCheckListMaxAvailableModel model;
            TimeSpan timeUsed;
            (model, timeUsed) = await GetMaxAvailableUsingClientWithAccess();

            ShowModel("GetMaxAvailable", model, timeUsed);
            Assert.IsTrue(model.MaxAvailable >= 2800000);
        }

        //[Ignore("Very long running test. To be used during development at localhost")]
        [TestMethod]
        public async Task C_ShouldGetSameMaxAvailableIfHasAccess()
        {
            var results = new List<long>();

            for (var idx = 0; idx < 5; idx++)
            {
                PbiCheckListMaxAvailableModel model;
                TimeSpan timeUsed;
                (model, timeUsed) = await GetMaxAvailableUsingClientWithAccess();

                ShowModel($"GetMaxAvailable, try {idx}", model, timeUsed);
                results.Add(model.MaxAvailable);
            }

            for (var idx = 1; idx < results.Count; idx++)
            {
                // max available should be constant
                var result = results[idx];
                Assert.AreEqual(results[idx-0], result);
                Assert.IsTrue(result >= 2800000);
            }
        }

        private async Task<(PbiCheckListMaxAvailableModel, TimeSpan)> GetMaxAvailableUsingClientWithAccess()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var checkListModel = await CheckListTestsHelper.GetMaxAvailable(ClientWithAccess);
            stopWatch.Stop();
            return (checkListModel, stopWatch.Elapsed);
        }

        private static void ShowModel(string message, PbiCheckListMaxAvailableModel model, TimeSpan timeUsedTotal)
        {
            Console.WriteLine(message);
            Console.WriteLine($"Time used for counting {model.MaxAvailable} records from PBI$CHECKLIST: {model.TimeUsed}");
            var timeUsed = $"{timeUsedTotal.Hours:00}h {timeUsedTotal.Minutes:00}m {timeUsedTotal.Seconds:00}s";
            Console.WriteLine($"Time used total incl networking: {timeUsed}");
        }
    }
}
