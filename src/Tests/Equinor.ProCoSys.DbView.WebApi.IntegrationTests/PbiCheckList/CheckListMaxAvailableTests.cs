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
        [TestCategory("All")]
        [TestMethod]
        public async Task A1_ShouldReturnUnauthorizedIfNotAuthenticated()
            => await CheckListTestsHelper.GetMaxAvailable(NotAuthenticatedRestClient, null, HttpStatusCode.Unauthorized);

        [TestCategory("All")]
        [TestMethod]
        public async Task A2_ShouldReturnForbiddenIfNoAccess()
            => await CheckListTestsHelper.GetMaxAvailable(ClientWithoutAnyRoles, null, HttpStatusCode.Forbidden);

        [TestCategory("Test")]
        [TestMethod]
        public async Task B_ShouldGetMaxAvailable()
        {
            PbiCheckListMaxAvailableModel model;
            TimeSpan timeUsed;
            (model, timeUsed) = await GetMaxAvailableUsingClientWithAccess();

            ShowModel("GetMaxAvailable", model, timeUsed);
            // total number of checklist pr Nov 2021 was 2665754
            Assert.IsTrue(model.MaxAvailable >= 2600000);
        }

        [TestCategory("Local")]
        [TestMethod]
        public async Task C_ShouldGetSameMaxAvailable()
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

            // test can be unstable due to possible added checklists during test runtime
            for (var idx = 1; idx < results.Count; idx++)
            {
                // max available should be constant
                var result = results[idx];
                Assert.AreEqual(results[idx-0], result);
            }
        }

        [TestCategory("Test")]
        [TestMethod]
        public async Task D_ShouldGetMaxAvailableWithCutoffDate()
        {
            PbiCheckListMaxAvailableModel model;
            TimeSpan timeUsed;
            const int daysOffset = 60;
            (model, timeUsed) = await GetMaxAvailableUsingClientWithAccess(CheckListTestsHelper.CreateDateOffsetToday(daysOffset*-1));

            ShowModel("GetMaxAvailable", model, timeUsed);
            var maxExpectedChangesPastDays = 200000;
            Assert.IsTrue(model.MaxAvailable < maxExpectedChangesPastDays, 
                $"Number of changed checklists {model.MaxAvailable} is more than {maxExpectedChangesPastDays} past {daysOffset} days. Can be natural. Consider modify the test");
        }

        [TestCategory("Test")]
        [TestMethod]
        public async Task E_ShouldGetZeroWithFutureCutoffDate()
        {
            PbiCheckListMaxAvailableModel model;
            TimeSpan timeUsed;
            (model, timeUsed) = await GetMaxAvailableUsingClientWithAccess(CheckListTestsHelper.CreateDateOffsetToday(1));

            ShowModel("GetMaxAvailable", model, timeUsed);
            Assert.AreEqual(0, model.MaxAvailable);
        }

        private async Task<(PbiCheckListMaxAvailableModel, TimeSpan)> GetMaxAvailableUsingClientWithAccess(string cutoffDate = null)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var checkListModel = await CheckListTestsHelper.GetMaxAvailable(ClientWithAccess, cutoffDate);
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
