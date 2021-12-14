using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag
{
    [AuthorizeAnyRole("DBView.3DEco.Read.All")]
    [ApiController]
    [Route("api/DbView/3DEcosystem")]
    public class ThreeDEcoTagController : ControllerBase
    {
        private readonly ITagRepository _repo;

        public ThreeDEcoTagController(ITagRepository repo) => _repo = repo;

        /// <summary>
        /// POC! Get page of Tags available for 3D Ecosystems POC!
        /// </summary>
        /// <param name="installationCode">Installation code, representing the plant (I.e JSV, GRA, TROA etc...)</param>
        /// <param name="currentPage">Current page to get. Default is 0 (first page)</param>
        /// <param name="itemsPerPage">Number of items pr page. Default is 100000</param>
        /// <param name="max">Max records to return. For swagger testing to avoid freezing swagger. Default = 0 will return all</param>
        /// <remarks>This is a POC. Can be changed or removed at any time</remarks>
        /// <response code="200">OK</response>
        [SwaggerOperation(Tags = new[] { "3DEcosystem" })]
        [HttpGet]
        public TagModel GetPage(string installationCode, int currentPage = 0, int itemsPerPage = 100000, int max = 0)
        {
            var model = _repo.GetPage(installationCode, currentPage, itemsPerPage, max);
            return model;
        }
    }
}
