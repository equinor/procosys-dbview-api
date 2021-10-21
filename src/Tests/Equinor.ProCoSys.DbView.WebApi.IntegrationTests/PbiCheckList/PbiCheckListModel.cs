using System.Collections.Generic;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.PbiCheckList
{
    public class PbiCheckListModel
    {
        public string CheckListUrlFormat { get; set; }
        public string TimeUsed { get; set; }
        public long Count { get; set; }
        public IEnumerable<CheckListInstance> CheckLists { get; set; }
    }
}
