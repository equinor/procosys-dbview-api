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
        public async Task A1_ShouldReturnUnauthorizedIfNotAuthenticated()
            => await TagTestsHelper.GetTagPage(NotAuthenticatedRestClient, null, 0, 10, HttpStatusCode.Unauthorized);
        
        [TestCategory("All")]
        [TestMethod]
        public async Task A2_ShouldReturnForbiddenIfNoAccess()
            => await TagTestsHelper.GetTagPage(ClientWithoutAnyRoles, null, 0, 10, HttpStatusCode.Forbidden);

        [TestCategory("Local")]
        [TestMethod]
        public async Task B1_ShouldGetAllTagPages()
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
                getNextPage = nextPage.Tags.Count() == itemsPerPage;
            }

            // total number of tags in Johan Sverdrup pr Jan 2022 was 654899
            Assert.IsTrue(page >= 2);
        }

        [TestCategory("Local")]
        [TestMethod]
        public async Task B2_ShouldGetRandomTagPages()
        {
            const int itemsPerPage = 100000;
            TimeSpan timeUsed;
            TagModel prevPage;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccess(0, itemsPerPage);
            ShowModel("Page 0", prevPage, timeUsed);
            AssertModel(prevPage, itemsPerPage);
            
            var pages = new [] {1, 5, 2};
            foreach (var page in pages)
            {
                TagModel nextPage;
                (nextPage, timeUsed) = await GetPageUsingClientWithAccess(page, itemsPerPage);
                ShowModel($"Page {page}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage);

                AssertDifferentPages(prevPage, nextPage);

                prevPage = nextPage;
            }
        }

        [TestCategory("Local")]
        [TestMethod]
        public async Task D_ShouldGetSameTagPage()
        {
            const int page = 0;
            const int itemsPerPage = 10000;
            TimeSpan timeUsed;
            TagModel prevPage;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccess(0, itemsPerPage);
            ShowModel("Page 0", prevPage, timeUsed);
            AssertModel(prevPage, itemsPerPage);

            for (var idx = 1; idx < 3; idx++)
            {
                TagModel nextPage;
                (nextPage, timeUsed) = await GetPageUsingClientWithAccess(page, itemsPerPage);
                ShowModel($"Page {page}, try #{idx}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage);

                AssertEqualPages(prevPage, nextPage);

                prevPage = nextPage;
            }
        }
        
        [TestCategory("Test")]
        [TestMethod]
        public async Task C_ShouldGetEmptyTagPageFromPageBehindLastPage()
        {
            const int itemsPerPage = 100000;
            TimeSpan timeUsed;
            TagModel prevPage;
            var page = 10;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccess(page, itemsPerPage);
            ShowModel($"Page {page}", prevPage, timeUsed);
            AssertModel(prevPage, 0);
        }
        
        [TestCategory("Test")]
        [TestMethod]
        public async Task E_ShouldGetSmallTagPage()
        {
            const int itemsPerPage = 100;
            TimeSpan timeUsed;
            TagModel page0;

            (page0, timeUsed) = await GetPageUsingClientWithAccess(0, itemsPerPage);
            ShowModel("Page 0", page0, timeUsed);
            AssertModel(page0, itemsPerPage);
        }

        [TestCategory("Test")]
        [TestMethod]
        public async Task F_ShouldGetBigTagPage()
        {
            const int itemsPerPage = 500000;
            TimeSpan timeUsed;
            TagModel page0;

            (page0, timeUsed) = await GetPageUsingClientWithAccess(0, itemsPerPage);
            ShowModel("Page 0", page0, timeUsed);
            AssertModel(page0, itemsPerPage);
        }

        private static void AssertEqualPages(TagModel prevPage, TagModel nextPage)
        {
            var tagNoIdx = GetAndVerifyColumnIdx(prevPage.Heading, 0, "TagNo");
            var projectIdx = GetAndVerifyColumnIdx(prevPage.Heading, 1, "Project");
            var tagsPrevPage = prevPage.Tags.Select(tagData => GetUniqeKeyForTag(tagData, tagNoIdx, projectIdx)).Distinct().ToList();
            var tagsNextPage = nextPage.Tags.Select(tagData => GetUniqeKeyForTag(tagData, tagNoIdx, projectIdx)).Distinct().ToList();

            // all items on prev and next page be equal => 
            Assert.AreEqual(tagsPrevPage.Count, tagsNextPage.Count);
            for (var i = 0; i < tagsPrevPage.Count; i++)
            {
                Assert.AreEqual(tagsPrevPage[i], tagsNextPage[i]);
            }
        }

        private static string GetUniqeKeyForTag(IEnumerable<object> tagData, int tagNoIdx, int projectIdx)
            => $"{tagData.ElementAt(tagNoIdx)}_{tagData.ElementAt(projectIdx)}";

        private static int GetAndVerifyColumnIdx(IEnumerable<string> heading, int colIdx, string colName)
        {
            var col = heading.ElementAt(colIdx);
            Assert.AreEqual(colName, col);
            return colIdx;
        }

        private static void AssertDifferentPages(TagModel prevPage, TagModel nextPage)
        {
            var tagNoIdx = GetAndVerifyColumnIdx(prevPage.Heading, 0, "TagNo");
            var projectIdx = GetAndVerifyColumnIdx(prevPage.Heading, 1, "Project");
            var tagsPrevPage = prevPage.Tags.Select(tagData => GetUniqeKeyForTag(tagData, tagNoIdx, projectIdx)).Distinct().ToList();
            var tagsNextPage = nextPage.Tags.Select(tagData => GetUniqeKeyForTag(tagData, tagNoIdx, projectIdx)).Distinct().ToList();
            var allDistinctTags = tagsPrevPage.Concat(tagsNextPage).Distinct();

            Assert.AreEqual(tagsPrevPage.Count + tagsNextPage.Count, allDistinctTags.Count());
        }

        private async Task<(TagModel, TimeSpan)> GetPageUsingClientWithAccess(int currentPage, int itemsPerPage)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var TagModel = await TagTestsHelper.GetTagPage(ClientWithAccess, Config.InstCodeUnderTest, currentPage, itemsPerPage);
            stopWatch.Stop();
            return (TagModel, stopWatch.Elapsed);
        }

        private static void ShowModel(string message, TagModel model, TimeSpan timeUsedTotal)
        {
            var count = model.Tags.Count();

            Console.WriteLine($"{message} ({DateTime.Now})");
            Console.WriteLine($"Time used for fetch {count} records from HOLO$COMMPKG_TAG: {model.TimeUsed}");
            var timeUsed = $"{timeUsedTotal.Hours:00}h {timeUsedTotal.Minutes:00}m {timeUsedTotal.Seconds:00}s";
            Console.WriteLine($"Time used total incl networking: {timeUsed}");
            Console.Write("Heading: ");
            foreach (var head in model.Heading)
            {
                Console.Write($"{head} ");
            }
            Console.WriteLine("");

            var tagNoIdx = GetAndVerifyColumnIdx(model.Heading, 0, "TagNo");
            var projectIdx = GetAndVerifyColumnIdx(model.Heading, 1, "Project");
            if (count > 0)
            {
                Console.WriteLine($"First tag {GetUniqeKeyForTag(model.Tags.First(), tagNoIdx, projectIdx)}");
            }
        }

        private static void AssertModel(TagModel model, int itemsPerPage, bool assertCount = true)
        {
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.Heading);
            Assert.IsNotNull(model.Heading.SingleOrDefault(h => h == "TagNo"));
            Assert.IsNotNull(model.Tags);

            var tagNoIdx = GetAndVerifyColumnIdx(model.Heading, 0, "TagNo");
            var projectIdx = GetAndVerifyColumnIdx(model.Heading, 1, "Project");
            var allTags = model.Tags.Select(tagData => GetUniqeKeyForTag(tagData, tagNoIdx, projectIdx)).Distinct().ToList();
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
