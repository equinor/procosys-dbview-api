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
            => await TagTestsHelper.GetTagPage(NotAuthenticatedRestClient, Config.InstCodeUnderTest, 0, 10, HttpStatusCode.Unauthorized);
        
        [TestCategory("All")]
        [TestMethod]
        public async Task A2_GetTagPage_ShouldReturnForbiddenIfNoAccess()
            => await TagTestsHelper.GetTagPage(ClientWithoutAnyRoles, Config.InstCodeUnderTest, 0, 10, HttpStatusCode.Forbidden);

        [TestCategory("All")]
        [TestMethod]
        public async Task B1_GetTagPage_ShouldGetAllTagPages()
        {
            const int itemsPerPage = 100000;
            var getNextPage = true;
            var page = 0;

            var instCode = Config.InstCodeUnderTest;

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

        [TestCategory("All")]
        [TestMethod]
        public async Task B2_GetTagPage_ShouldGetDifferentTagPages()
        {
            const int itemsPerPage = 50000;
            TimeSpan timeUsed;
            TagModel prevPage;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccessAsync(Config.InstCodeUnderTest, 0, itemsPerPage);
            ShowModel("Page 0", prevPage, timeUsed);
            AssertModel(prevPage, itemsPerPage);
            
            var pages = new [] {1, 5, 2};
            foreach (var page in pages)
            {
                TagModel nextPage;
                (nextPage, timeUsed) = await GetPageUsingClientWithAccessAsync(Config.InstCodeUnderTest, page, itemsPerPage);
                ShowModel($"Page {page}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage);

                AssertDifferentPages(prevPage, nextPage);

                prevPage = nextPage;
            }
        }

        [TestCategory("All")]
        [TestMethod]
        public async Task D_GetTagPage_ShouldGetSameTagPage()
        {
            const int page = 0;
            const int itemsPerPage = 10000;
            TimeSpan timeUsed;
            TagModel prevPage;
            var instCode = Config.InstCodeUnderTest;

            (prevPage, timeUsed) = await GetPageUsingClientWithAccessAsync(instCode, 0, itemsPerPage);
            ShowModel("Page 0", prevPage, timeUsed);
            AssertModel(prevPage, itemsPerPage);

            for (var idx = 1; idx < 3; idx++)
            {
                TagModel nextPage;
                (nextPage, timeUsed) = await GetPageUsingClientWithAccessAsync(instCode, page, itemsPerPage);
                ShowModel($"Page {page}, try #{idx}", nextPage, timeUsed);
                AssertModel(nextPage, itemsPerPage);

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
            var instCode = Config.RandomInstCodeUnderTest;

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
            var tagNoIdx = GetAndVerifyColumnIdx(prevPage.Heading, 0, "TagNo");
            var projectIdx = GetAndVerifyColumnIdx(prevPage.Heading, 1, "Project");
            var commPkgNoIdx = GetAndVerifyColumnIdx(prevPage.Heading, 3, "CommPkgNo");
            var responsibleIdx = GetAndVerifyColumnIdx(prevPage.Heading, 11, "Responsible");
            var tagsPrevPage = prevPage.Tags.Select(tagData => GetUniqeKeyForTag(tagData, tagNoIdx, projectIdx, commPkgNoIdx, responsibleIdx)).Distinct().ToList();
            var tagsNextPage = nextPage.Tags.Select(tagData => GetUniqeKeyForTag(tagData, tagNoIdx, projectIdx, commPkgNoIdx, responsibleIdx)).Distinct().ToList();

            // all items on prev and next page be equal => 
            Assert.AreEqual(tagsPrevPage.Count, tagsNextPage.Count);
            for (var i = 0; i < tagsPrevPage.Count; i++)
            {
                Assert.AreEqual(tagsPrevPage[i], tagsNextPage[i]);
            }
        }

        private static string GetUniqeKeyForTag(IEnumerable<object> tagData, int tagNoIdx, int projectIdx, int commPkgNoIdx, int responsibleIdx)
            => $"{tagData.ElementAt(tagNoIdx)}_{tagData.ElementAt(projectIdx)}_{tagData.ElementAt(commPkgNoIdx)}_{tagData.ElementAt(responsibleIdx)}";

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
            var commPkgNoIdx = GetAndVerifyColumnIdx(prevPage.Heading, 3, "CommPkgNo");
            var responsibleIdx = GetAndVerifyColumnIdx(prevPage.Heading, 11, "Responsible");
            var tagsPrevPage = prevPage.Tags.Select(tagData => GetUniqeKeyForTag(tagData, tagNoIdx, projectIdx, commPkgNoIdx, responsibleIdx)).Distinct().ToList();
            var tagsNextPage = nextPage.Tags.Select(tagData => GetUniqeKeyForTag(tagData, tagNoIdx, projectIdx, commPkgNoIdx, responsibleIdx)).Distinct().ToList();
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
            Console.WriteLine($"Time used for fetch {count} Tag records: {model.TimeUsed}");
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
            var commPkgNoIdx = GetAndVerifyColumnIdx(model.Heading, 3, "CommPkgNo");
            var responsibleIdx = GetAndVerifyColumnIdx(model.Heading, 11, "Responsible");
            if (count > 0)
            {
                Console.WriteLine($"First tag {GetUniqeKeyForTag(model.Tags.First(), tagNoIdx, projectIdx, commPkgNoIdx, responsibleIdx)}");
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
            var commPkgNoIdx = GetAndVerifyColumnIdx(model.Heading, 3, "CommPkgNo");
            var responsibleIdx = GetAndVerifyColumnIdx(model.Heading, 11, "Responsible");
            var allTags = model.Tags.Select(tagData => GetUniqeKeyForTag(tagData, tagNoIdx, projectIdx, commPkgNoIdx, responsibleIdx)).Distinct().ToList();
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
