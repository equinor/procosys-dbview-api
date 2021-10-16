using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.PbiCheckList
{
    public class PbiCheckListModel
    {
        /// <summary>
        /// Format for url to each checklist. Replace {PROJECTSCHEMA} (without PCS$) and {CHECKLIST_ID}
        /// </summary>
        public string CheckListUrlFormat { get; set; }
        
        /// <summary>
        /// Information about time used to querying records from database
        /// </summary>
        [Required]
        public string TimeUsed { get; set; }

        /// <summary>
        /// Number of records returned
        /// </summary>
        [Required] 
        public long Count { get; set; }

        /// <summary>
        /// List of checklist records
        /// </summary>
        [Required]
        public IEnumerable<CheckListInstance> CheckLists { get; set; }
    }
}
