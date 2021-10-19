﻿using System;
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
        [TestMethod]
        public async Task A1_ShouldReturnUnauthorizedIfNotAuthenticated()
            => await CheckListTestsHelper.GetCheckListPage(NotAuthenticatedRestClient, 0, 10, HttpStatusCode.Unauthorized);

        //[Ignore("Very long running test. To be used during development at localhost")]
        [TestMethod]
        public async Task E_ShouldGetSmallCheckListPages()
        {
            const int itemsPerPage = 100;
            TimeSpan timeUsed;
            PbiCheckListModel page0;

            (page0, timeUsed) = await GetPage(0, itemsPerPage);
            ShowModel("Page 0", page0, timeUsed);
            AssertModel(page0, itemsPerPage);
        }

        //[Ignore("Very long running test. To be used during development at localhost")]
        [TestMethod]
        public async Task F_ShouldGetBigCheckListPages()
        {
            const int itemsPerPage = 500000;
            TimeSpan timeUsed;
            PbiCheckListModel page0;

            (page0, timeUsed) = await GetPage(0, itemsPerPage);
            ShowModel("Page 0", page0, timeUsed);
            AssertModel(page0, itemsPerPage);
        }
        
        //[Ignore("Very long running test. To be used during development at localhost")]
        [TestMethod]
        public async Task C_ShouldGetZeroCheckListsFromPageBehindLastPage()
        {
            const int itemsPerPage = 100000;
            TimeSpan timeUsed;
            PbiCheckListModel prevPage;
            var page = 40;

            (prevPage, timeUsed) = await GetPage(page, itemsPerPage);
            ShowModel($"Page {page}", prevPage, timeUsed);
            AssertModel(prevPage, 0);
        }

        //[Ignore("Very long running test. To be used during development at localhost")]
        [TestMethod]
        public async Task B1_ShouldAllPagedCheckLists()
        {
            const int itemsPerPage = 100000;
            var getNextPage = true;
            var page = 0;

            while (getNextPage)
            {
                var (nextPage, timeUsed) = await GetPage(page, itemsPerPage);
                
                ShowModel($"Page {page}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage, false);

                page++;
                getNextPage = nextPage.CheckLists.Count() == itemsPerPage;
            }

            Assert.IsTrue(page >= 28);
        }

        //[Ignore("Very long running test. To be used during development at localhost")]
        [TestMethod]
        public async Task B2_ShouldGetRandomPagedCheckLists()
        {
            const int itemsPerPage = 100000;
            TimeSpan timeUsed;
            PbiCheckListModel prevPage;

            (prevPage, timeUsed) = await GetPage(0, itemsPerPage);
            ShowModel("Page 0", prevPage, timeUsed);
            AssertModel(prevPage, itemsPerPage);
            
            var pages = new [] {1, 5, 15, 20, 1};
            foreach (var page in pages)
            {
                PbiCheckListModel nextPage;
                (nextPage, timeUsed) = await GetPage(page, itemsPerPage);
                ShowModel($"Page {page}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage);

                AssertDifferentPages(prevPage, nextPage);

                prevPage = nextPage;
            }
        }

        //[Ignore("Very long running test. To be used during development at localhost")]
        [TestMethod]
        public async Task D_ShouldGetSameCheckListsPage()
        {
            const int page = 0;
            const int itemsPerPage = 100000;
            TimeSpan timeUsed;
            PbiCheckListModel prevPage;

            (prevPage, timeUsed) = await GetPage(0, itemsPerPage);
            ShowModel("Page 0", prevPage, timeUsed);
            AssertModel(prevPage, itemsPerPage);

            for (var idx = 1; idx < 5; idx++)
            {
                PbiCheckListModel nextPage;
                (nextPage, timeUsed) = await GetPage(page, itemsPerPage);
                ShowModel($"Page {page}, try #{idx}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage);

                AssertEqualPages(prevPage, nextPage);

                prevPage = nextPage;
            }
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

        private async Task<(PbiCheckListModel, TimeSpan)> GetPage(int currentPage, int itemsPerPage)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var checkListModel = await CheckListTestsHelper.GetCheckListPage(AuthenticatedRestClient, currentPage, itemsPerPage);
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
            
            // The view is modified Oct 7 2021 to return one row pr checklist id
            // Bug Found in view Oct 12 2021. Checklist id seem to appear in different commpkgs/mcpkgs.
            var allItems = model.CheckLists.Select(c => $"{c.CheckList_Id}|{c.CommPkgNo}|{c.McPkgNo}").ToList();
            var distinctItems = allItems.Distinct().ToList();
            if (allItems.Count != distinctItems.Count)
            {
                foreach (var item in distinctItems)
                {
                    var possibleDuplicate = allItems.Where(dup => dup == item);
                    if (possibleDuplicate.Count() > 1)
                    {
                        Console.WriteLine($"Duplicate item: {item}");
                    }
                }
            }

            Assert.AreEqual(allItems.Count, distinctItems.Count);

            if (assertCount)
            {
                Assert.AreEqual(itemsPerPage, model.CheckLists.Count());
            }
        }
    }
}
