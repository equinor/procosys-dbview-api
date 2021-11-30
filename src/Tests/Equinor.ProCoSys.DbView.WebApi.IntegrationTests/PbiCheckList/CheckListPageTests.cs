using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.PbiCheckList
{
    [TestClass]
    public class CheckListPageTests : ClientSetup
    {
        [TestCategory("All")]
        [TestMethod]
        public async Task A1_ShouldReturnUnauthorizedIfNotAuthenticated()
            => await CheckListTestsHelper.GetCheckListPage(NotAuthenticatedRestClient, null, 0, 10, HttpStatusCode.Unauthorized);
        
        [TestCategory("All")]
        [TestMethod]
        public async Task A2_ShouldReturnForbiddenIfNoAccess()
            => await CheckListTestsHelper.GetCheckListPage(ClientWithoutAnyRoles, null, 0, 10, HttpStatusCode.Forbidden);

        [TestCategory("Local")]
        [TestMethod]
        public async Task B1_ShouldGetAllCheckListPages()
        {
            const int itemsPerPage = 500000;
            var getNextPage = true;
            var page = 0;

            while (getNextPage)
            {
                var (nextPage, timeUsed) = await GetPageUsingClientWithAccess(page, itemsPerPage);
                
                ShowModel($"Page {page}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage, false);

                page++;
                getNextPage = nextPage.CheckLists.Count() == itemsPerPage;
            }

            // total number of chgecklist pr Nov 2021 was 2665754
            Assert.IsTrue(page >= 26);
        }

        [TestCategory("Local")]
        [TestMethod]
        public async Task B2_ShouldGetRandomCheckListPages()
        {
            const int itemsPerPage = 100000;
            TimeSpan timeUsed;
            PbiCheckListModel prevPage;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccess(0, itemsPerPage);
            ShowModel("Page 0", prevPage, timeUsed);
            AssertModel(prevPage, itemsPerPage);
            
            var pages = new [] {1, 5, 15, 20, 1};
            foreach (var page in pages)
            {
                PbiCheckListModel nextPage;
                (nextPage, timeUsed) = await GetPageUsingClientWithAccess(page, itemsPerPage);
                ShowModel($"Page {page}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage);

                AssertDifferentPages(prevPage, nextPage);

                prevPage = nextPage;
            }
        }

        [TestCategory("Local")]
        [TestMethod]
        public async Task D_ShouldGetSameCheckListPage()
        {
            const int page = 0;
            const int itemsPerPage = 100000;
            TimeSpan timeUsed;
            PbiCheckListModel prevPage;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccess(0, itemsPerPage);
            ShowModel("Page 0", prevPage, timeUsed);
            AssertModel(prevPage, itemsPerPage);

            for (var idx = 1; idx < 5; idx++)
            {
                PbiCheckListModel nextPage;
                (nextPage, timeUsed) = await GetPageUsingClientWithAccess(page, itemsPerPage);
                ShowModel($"Page {page}, try #{idx}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage);

                AssertEqualPages(prevPage, nextPage);

                prevPage = nextPage;
            }
        }
        
        [TestCategory("Test")]
        [TestMethod]
        public async Task C_ShouldGetEmptyCheckListPageFromPageBehindLastPage()
        {
            const int itemsPerPage = 100000;
            TimeSpan timeUsed;
            PbiCheckListModel prevPage;
            var page = 40;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccess(page, itemsPerPage);
            ShowModel($"Page {page}", prevPage, timeUsed);
            AssertModel(prevPage, 0);
        }
        
        [TestCategory("Test")]
        [TestMethod]
        public async Task E_ShouldGetSmallCheckListPage()
        {
            const int itemsPerPage = 100;
            TimeSpan timeUsed;
            PbiCheckListModel page0;

            (page0, timeUsed) = await GetPageUsingClientWithAccess(0, itemsPerPage);
            ShowModel("Page 0", page0, timeUsed);
            AssertModel(page0, itemsPerPage);
        }

        [TestCategory("Test")]
        [TestMethod]
        public async Task F_ShouldGetBigCheckListPage()
        {
            const int itemsPerPage = 500000;
            TimeSpan timeUsed;
            PbiCheckListModel page0;

            (page0, timeUsed) = await GetPageUsingClientWithAccess(0, itemsPerPage);
            ShowModel("Page 0", page0, timeUsed);
            AssertModel(page0, itemsPerPage);
        }

        [TestCategory("Test")]
        [TestMethod]
        public async Task G1_ShouldGetCheckListPageWithCutoffDate()
        {
            const int daysOffset = 60;
            const int itemsPerPage = 200000;
            TimeSpan timeUsed;
            PbiCheckListModel page0;

            (page0, timeUsed) = await GetPageUsingClientWithAccess(0, itemsPerPage, CheckListTestsHelper.CreateDateOffsetToday(daysOffset*-1));
            ShowModel("Page 0", page0, timeUsed);
            AssertModel(page0, itemsPerPage, false);
            Assert.IsTrue(page0.CheckLists.Count() < itemsPerPage,
                $"Number of changed checklists {page0.CheckLists.Count()} is more than {itemsPerPage} past {daysOffset} days. Can be natural. Consider modify the test");
        }

        [TestCategory("Test")]
        [TestMethod]
        public async Task G2_ShouldGetEmptyCheckListPageWithFutureCutoffDate()
        {
            const int itemsPerPage = 100000;
            TimeSpan timeUsed;
            PbiCheckListModel page0;

            (page0, timeUsed) = await GetPageUsingClientWithAccess(0, itemsPerPage, CheckListTestsHelper.CreateDateOffsetToday(1));
            ShowModel("Page 0", page0, timeUsed);
            AssertModel(page0, 0);
        }

        private static void AssertEqualPages(PbiCheckListModel prevPage, PbiCheckListModel nextPage)
        {
            var idsPrevPage = prevPage.CheckLists.Select(c => c.CheckList_Id).Distinct().ToList();
            var idsNextPage = nextPage.CheckLists.Select(c => c.CheckList_Id).Distinct().ToList();

            // all items on prev and next page be equal => 
            Assert.AreEqual(idsPrevPage.Count, idsNextPage.Count);

            for (var i = 0; i < idsPrevPage.Count; i++)
            {
                Assert.AreEqual(idsPrevPage[i], idsNextPage[i]);
            }
        }
        
        private static void AssertDifferentPages(PbiCheckListModel prevPage, PbiCheckListModel nextPage)
        {
            var idsPrevPage = prevPage.CheckLists.Select(c => c.CheckList_Id).Distinct().ToList();
            var idsNextPage = nextPage.CheckLists.Select(c => c.CheckList_Id).Distinct().ToList();
            var allDistinctIds = idsPrevPage.Concat(idsNextPage).Distinct();

            Assert.AreEqual(idsPrevPage.Count + idsNextPage.Count, allDistinctIds.Count());
        }

        private async Task<(PbiCheckListModel, TimeSpan)> GetPageUsingClientWithAccess(int currentPage, int itemsPerPage, string cutoffDate = null)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var checkListModel = await CheckListTestsHelper.GetCheckListPage(ClientWithAccess, cutoffDate, currentPage, itemsPerPage);
            stopWatch.Stop();
            return (checkListModel, stopWatch.Elapsed);
        }

        private static void ShowModel(string message, PbiCheckListModel model, TimeSpan timeUsedTotal)
        {
            var count = model.CheckLists.Count();

            Console.WriteLine($"{message} ({DateTime.Now})");
            Console.WriteLine($"Time used for fetch {count} records from PBI$CHECKLIST: {model.TimeUsed}");
            var timeUsed = $"{timeUsedTotal.Hours:00}h {timeUsedTotal.Minutes:00}m {timeUsedTotal.Seconds:00}s";
            Console.WriteLine($"Time used total incl networking: {timeUsed}");
            if (count > 0)
            {
                Console.WriteLine($"First id {model.CheckLists.First().CheckList_Id}");
            }
        }

        private static void AssertModel(PbiCheckListModel model, int itemsPerPage, bool assertCount = true)
        {
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.CheckLists);
            
            // The view is modified Oct 2021 to return one row pr checklist id
            var allIds = model.CheckLists.Select(c => c.CheckList_Id).ToList();
            var distinctIds = allIds.Distinct().ToList();
            if (allIds.Count != distinctIds.Count)
            {
                foreach (var item in distinctIds)
                {
                    var possibleDuplicate = allIds.Where(dup => dup == item);
                    if (possibleDuplicate.Count() > 1)
                    {
                        Console.WriteLine($"Duplicate item: {item}");
                    }
                }
            }

            Assert.AreEqual(allIds.Count, distinctIds.Count);

            if (assertCount)
            {
                Assert.AreEqual(itemsPerPage, model.CheckLists.Count());
            }
        }
    }
}
