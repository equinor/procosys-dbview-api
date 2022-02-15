using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.ThreeDEcoTag
{
    [TestClass]
    public class TagPageTests : ClientSetup
    {
        [TestCategory("All")]
        [TestMethod]
        public async Task A1_GetTagPage_ShouldReturnUnauthorizedIfNotAuthenticated()
            => await TagTestsHelper.GetTagPage(NotAuthenticatedRestClient, Config.InstCodeUnderTest_Large, 0, 10, HttpStatusCode.Unauthorized);
        
        [TestCategory("All")]
        [TestMethod]
        public async Task A2_GetTagPage_ShouldReturnForbiddenIfNoAccess()
            => await TagTestsHelper.GetTagPage(ClientWithoutAnyRoles, Config.InstCodeUnderTest_Large, 0, 10, HttpStatusCode.Forbidden);

        [TestCategory("Local")]
        [TestCategory("Prod")]
        // Dont run in test yet. Correct data misses there
        [TestMethod]
        public async Task B_GetTagPage_ShouldGetCorrectTagData()
        {
            const int itemsPerPage = 500;
            TimeSpan timeUsed;
            TagModel page0;
            var instCode = Config.InstCodeUnderTest_Static;

            (page0, timeUsed) = await GetPageUsingClientWithAccessAsync(instCode, 0, itemsPerPage);
            ShowModel("Page 0", page0, timeUsed);
            AssertModel(page0, itemsPerPage, false);

            Assert.IsTrue(itemsPerPage > page0.Tags.Count());

            AssertKnownDetails(page0);
        }

        [TestCategory("All")]
        [TestMethod]
        public async Task C1_GetTagPage_ShouldGetAllTagPages()
        {
            const int itemsPerPage = 100000;
            var getNextPage = true;
            var page = 0;

            var instCode = Config.InstCodeUnderTest_Large;

            while (getNextPage)
            {
                var (nextPage, timeUsed) = await GetPageUsingClientWithAccessAsync(instCode, page, itemsPerPage);
                
                ShowModel($"Page {page}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage, false);

                page++;
                getNextPage = nextPage.Tags.Count() == itemsPerPage;
            }

            // total number of tags for JSV pr Feb 2022 was 474514
            Assert.IsTrue(page >= 3);
        }

        [TestCategory("Local")]
        [TestCategory("Prod")]
        // Dont run in test yet. Correct data misses there
        [TestMethod]
        public async Task C2_GetTagPage_ShouldGetDifferentTagPages()
        {
            const int itemsPerPage = 10;
            TimeSpan timeUsed;
            TagModel prevPage;
            var instCode = Config.InstCodeUnderTest_Static;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccessAsync(instCode, 0, itemsPerPage);
            ShowModel("Page 0", prevPage, timeUsed);
            AssertModel(prevPage, itemsPerPage);
            
            var pages = new [] {1, 5, 2};
            foreach (var page in pages)
            {
                TagModel nextPage;
                (nextPage, timeUsed) = await GetPageUsingClientWithAccessAsync(instCode, page, itemsPerPage);
                ShowModel($"Page {page}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage);

                // These tests will fail if data changes will test run. Use TSTG
                AssertDifferentPages(prevPage, nextPage);

                prevPage = nextPage;
            }
        }

        [TestCategory("Local")]
        [TestCategory("Prod")]
        // Dont run in test yet. Correct data misses there
        [TestMethod]
        public async Task D_GetTagPage_ShouldGetSameTagPage()
        {
            const int page = 0;
            const int itemsPerPage = 50;
            TimeSpan timeUsed;
            TagModel prevPage;
            var instCode = Config.InstCodeUnderTest_Static;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccessAsync(instCode, 0, itemsPerPage);
            ShowModel("Page 0", prevPage, timeUsed);
            AssertModel(prevPage, itemsPerPage);

            for (var idx = 1; idx < 3; idx++)
            {
                TagModel nextPage;
                (nextPage, timeUsed) = await GetPageUsingClientWithAccessAsync(instCode, page, itemsPerPage);
                ShowModel($"Page {page}, try #{idx}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage);

                // These tests will fail if data changes will test run. Use TSTG
                AssertEqualPages(prevPage, nextPage);

                prevPage = nextPage;
            }
        }

        [TestCategory("All")]
        [TestMethod]
        public async Task C_GetTagPage_ShouldGetEmptyTagPageFromPageBehindLastPage()
        {
            const int itemsPerPage = 100000;
            TimeSpan timeUsed;
            TagModel prevPage;
            var page = 10;
            var instCode = Config.InstCodeUnderTest_Small;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccessAsync(instCode, page, itemsPerPage);
            ShowModel($"Page {page}", prevPage, timeUsed);
            AssertModel(prevPage, 0);
        }

        [TestCategory("All")]
        [TestMethod]
        public async Task E_GetTagPage_ShouldGetSmallTagPage()
        {
            const int itemsPerPage = 5;
            TimeSpan timeUsed;
            TagModel page0;
            var instCode = Config.RandomInstCodeUnderTest;

            (page0, timeUsed) = await GetPageUsingClientWithAccessAsync(instCode, 0, itemsPerPage);
            ShowModel("Page 0", page0, timeUsed);
            AssertModel(page0, itemsPerPage);
        }

        private static void AssertEqualPages(TagModel prevPage, TagModel nextPage)
        {
            var helper = new DetailsHelper(prevPage);

            var tagsPrevPage = prevPage.Tags.Select(tagData => helper.GetUniqeKeyForTag(tagData)).Distinct().ToList();
            var tagsNextPage = nextPage.Tags.Select(tagData => helper.GetUniqeKeyForTag(tagData)).Distinct().ToList();

            Assert.AreEqual(tagsPrevPage.Count, tagsNextPage.Count);

            // all items on prev and next page be equal => 
            Assert.AreEqual(tagsPrevPage.Count, tagsNextPage.Count);
            for (var i = 0; i < tagsPrevPage.Count; i++)
            {
                Assert.AreEqual(tagsPrevPage[i], tagsNextPage[i]);
            }
        }

        private static void AssertDifferentPages(TagModel prevPage, TagModel nextPage)
        {
            var helper = new DetailsHelper(prevPage);
            var tagsPrevPage = prevPage.Tags.Select(tagData => helper.GetUniqeKeyForTag(tagData)).Distinct().ToList();
            var tagsNextPage = nextPage.Tags.Select(tagData => helper.GetUniqeKeyForTag(tagData)).Distinct().ToList();
            var allDistinctTags = tagsPrevPage.Concat(tagsNextPage).Distinct();

            Assert.AreEqual(tagsPrevPage.Count + tagsNextPage.Count, allDistinctTags.Count());
        }

        private async Task<(TagModel, TimeSpan)> GetPageUsingClientWithAccessAsync(string installationCode, int currentPage, int itemsPerPage)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Console.WriteLine($"{DateTime.Now}: Getting {itemsPerPage} tags at page {currentPage} from '{installationCode}'");
            var TagModel = await TagTestsHelper.GetTagPage(ClientWithAccess, installationCode, currentPage, itemsPerPage);
            stopWatch.Stop();
            return (TagModel, stopWatch.Elapsed);
        }

        private static void ShowModel(string message, TagModel model, TimeSpan timeUsedTotal)
        {
            var count = model.Tags.Count();

            Console.WriteLine($"{message} ({DateTime.Now})");
            Console.WriteLine($"Time used for fetch {count} Tag records: Tags: {model.TimeUsedGettingTags}. CommPkgs: {model.TimeUsedGettingCommPkgs}. Total: {model.TimeUsedTotal}. ");
            var timeUsed = $"{timeUsedTotal.Hours:00}h {timeUsedTotal.Minutes:00}m {timeUsedTotal.Seconds:00}s";
            Console.WriteLine($"Time used from client side: {timeUsed}");
            Console.Write("Heading: ");
            foreach (var head in model.Heading)
            {
                Console.Write($"{head} ");
            }
            Console.WriteLine("");

            var helper = new DetailsHelper(model);
            if (count > 0)
            {
                Console.WriteLine($"First tag {helper.GetUniqeKeyForTag(model.Tags.First())}");
            }
        }

        private void AssertKnownDetails(TagModel model)
        {
            var helper = new DetailsHelper(model);

            AssertTagDetails(
                model,
                helper,
                "82EN650A-J87",
                "M.TST01.XX.0001",
                "9801-A01",
                "AATSTF",
                "9801-J005",
                "Not sent",
                "Not sent");

            AssertTagDetails(
                model,
                helper,
                "98EU601",
                "M.TST01.XX.0001",
                "9801-A02",
                "AATSTF",
                "9801-M001",
                "Sent",
                "Not sent");

            AssertTagDetails(
                model,
                helper,
                "98EU601-B01",
                "M.TST01.XX.0001",
                "9801-A03",
                "AATSTF",
                "9801-E008",
                "Rejected",
                "Not sent");

            AssertTagDetails(
                model,
                helper,
                "82EN650B-J91",
                "M.TST01.XX.0001",
                "9801-A04",
                "AATSTF",
                "9801-J006",
                "Fully accepted",
                "Not sent");

            AssertTagDetails(
                model,
                helper,
                "98EU602",
                "M.TST01.XX.0001",
                "9801-A05",
                "AATSTF",
                "9801-M002",
                "Partly accepted",
                "Not sent");

            AssertTagDetails(
                model,
                helper,
                "98EU603",
                "M.TST01.XX.0001",
                "9801-A06",
                "AATSTF",
                "9801-M003",
                "Fully accepted",
                "Sent");

            AssertTagDetails(
                model,
                helper,
                "98EU602-B01",
                "M.TST01.XX.0001",
                "9801-A07",
                "AATSTF",
                "9801-E009",
                "Fully accepted",
                "Rejected");

            AssertTagDetails(
                model,
                helper,
                "98EU603-B01",
                "M.TST01.XX.0001",
                "9801-A09",
                "AATSTF",
                "9801-E010",
                "Fully accepted",
                "Fully accepted");

            AssertTagDetails(
                model,
                helper,
                "98EU604",
                "M.TST01.XX.0001",
                "9801-A10",
                "AATSTF",
                "9801-M004",
                "Partly accepted",
                "Partly accepted");
        }

        private void AssertTagDetails(
            TagModel model,
            DetailsHelper helper,
            string tagNo,
            string project,
            string commPkgNo, 
            string responsible,
            string mcPkgNo,
            string rfcc, 
            string rfoc)
        {
            var tagDataToFind = $"{tagNo}_{project}_{commPkgNo}_{responsible}";
            var props = model
                .Tags
                .Where(tagData =>
                helper.GetUniqeKeyForTag(tagData) == tagDataToFind)
                .SingleOrDefault();
            Assert.IsNotNull(props, $"Didn't find expected {tagDataToFind}");
            Assert.AreEqual(mcPkgNo, props.ElementAt(helper.McPkgNoIdx));
            Assert.AreEqual(rfcc, props.ElementAt(helper.RfccIdx));
            Assert.AreEqual(rfoc, props.ElementAt(helper.RfocIdx));
        }

        private static void AssertModel(TagModel model, int itemsPerPage, bool assertCount = true)
        {
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Heading);
            Assert.IsNotNull(model.Heading.SingleOrDefault(h => h == "TagNo"));
            Assert.IsNotNull(model.Tags);

            var helper = new DetailsHelper(model);
            var allTags = model.Tags.Select(tagData => helper.GetUniqeKeyForTag(tagData)).Distinct().ToList();
            var distinctTags = allTags.Distinct().ToList();
            if (allTags.Count != distinctTags.Count)
            {
                foreach (var tag in distinctTags)
                {
                    var possibleDuplicate = allTags.Where(dup => dup == tag);
                    if (possibleDuplicate.Count() > 1)
                    {
                        Console.WriteLine($"Duplicate item: {tag}");
                    }
                }
            }

            Assert.AreEqual(allTags.Count, distinctTags.Count);

            if (assertCount)
            {
                Assert.AreEqual(itemsPerPage, model.Tags.Count());
            }
        }
    }
}
