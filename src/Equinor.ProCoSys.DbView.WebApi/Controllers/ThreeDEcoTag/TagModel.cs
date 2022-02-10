using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag
{
    public class TagModel
    {
        /// <summary>
        /// Information about time used to querying records from database
        /// </summary>
        [Required]
        public string TimeUsedGettingTags { get; set; }
        public string TimeUsedGettingCommPkgs { get; set; }
        public string TimeUsedGettingTotal { get; set; }

        /// <summary>
        /// Number of records returned
        /// </summary>
        [Required] 
        public long Count { get; set; }

        /// <summary>
        /// Heading, describing columns in Body
        /// </summary>
        [Required]
        public IEnumerable<string> Heading { get; set; }

        /// <summary>
        /// List of tags
        /// </summary>
        [Required]
        public IEnumerable<IEnumerable<object>> Tags { get; set; }
    }
}
