using System.Collections.Generic;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag
{
    public class TagPropertiesInstance
    {
        public TagPropertiesInstance(
            string tagNo,
            string project,
            int punchCount,
            string commPkgNo,
            string commPkgDesc,
            string mcPkgNo,
            string mcPkgDesc,
            string priority,
            string phase,
            string rfcc,
            string rfoc,
            string responsible,
            string status)
            => Props = new List<object>
            {
                tagNo,
                project,
                punchCount,
                commPkgNo,
                commPkgDesc,
                mcPkgNo,
                mcPkgDesc,
                priority,
                phase,
                rfcc,
                rfoc,
                responsible,
                status
            };

        public IEnumerable<object> Props { get; }
    }
}
