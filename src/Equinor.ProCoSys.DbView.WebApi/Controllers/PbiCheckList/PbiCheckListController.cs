using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.PbiCheckList
{
    [Authorize(Roles = "DbView.CheckList.Read.All")]
    [ApiController]
    [Route("api/DbView/PbiCheckList")]
    public class PbiCheckListController : ControllerBase
    {
        private readonly IPbiCheckListRepository _repo;

        public PbiCheckListController(IPbiCheckListRepository repo) => _repo = repo;

        /// <summary>
        /// POC! Count available CheckList records in PBI$CHECKLIST view (Test and Prod environment only) POC!
        /// </summary>
        /// <param name="cutoffDate">Get records changed after given cutoffDate. Default = null will return all</param>
        /// <remarks>This is a POC. Can be changed or removed at any time</remarks>
        /// <response code="200">OK</response>
        [HttpGet("Count")]
        public PbiCheckListMaxAvailableModel Count(DateTime? cutoffDate = null)
        {
            var model = _repo.GetMaxAvailable(cutoffDate);
            return model;
        }

        /// <summary>
        /// POC! Get all CheckList records in PBI$CHECKLIST view (Test and Prod environment only) POC!
        /// </summary>
        /// <param name="currentPage">Current page to get. Default is 0 (first page)</param>
        /// <param name="itemsPerPage">Number of items pr page. Default is 100000</param>
        /// <param name="cutoffDate">Get records changed after given cutoffDate. Default = null will return all</param>
        /// <param name="max">Max records to return. For swagger testing to avoid freezing swagger. Default = 0 will return all</param>
        /// <remarks>This is a POC. Can be changed or removed at any time</remarks>
        /// <response code="200">OK</response>
        [HttpGet]
        public PbiCheckListModel GetPage(int currentPage = 0, int itemsPerPage = 100000, DateTime? cutoffDate = null, int max = 0)
        {
            var model = _repo.GetPage(cutoffDate, currentPage, itemsPerPage, max);
            return model;
        }
    }
}
