using System.Collections.Generic;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.ThreeDEcoTag
{
    public class TagModel
    {
        public string TimeUsed { get; set; }
        public long Count { get; set; }
        public IEnumerable<string> Heading { get; set; }
        public IEnumerable<IEnumerable<object>> Tags { get; set; }
    }
}
