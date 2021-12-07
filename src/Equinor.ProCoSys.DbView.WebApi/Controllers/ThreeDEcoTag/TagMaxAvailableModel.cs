using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag
{
    public class TagMaxAvailableModel
    {
        /// <summary>
        /// Information about time used to count records available from database
        /// </summary>
        [Required]
        public string TimeUsed { get; set; }

        /// <summary>
        /// Number of records available
        /// </summary>
        [Required] 
        public long MaxAvailable { get; set; }
    }
}
