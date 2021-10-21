using System;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.DbView.WebApi.Controllers.PbiCheckList
{
    public class CheckListInstance
    {
        [Required]
        // ReSharper disable once InconsistentNaming
        public long CheckList_Id { get; set; }

        [Required]
        public string Projectschema { get; set; }

        [Required]
        public string Project { get; set; }

        [Required]
        public string TagNo { get; set; }

        // ReSharper disable once InconsistentNaming
        [Required]
        public string Tag_Category { get; set; }

        [Required]
        public string FormularType { get; set; }

        [Required]
        public string FormularGroup { get; set; }

        [Required]
        public string FormularDiscipline { get; set; }

        [Required]
        public string Responsible { get; set; }

        [Required]
        public string Status { get; set; }

        // ReSharper disable once InconsistentNaming
        [Required]
        public DateTime Created_At_Date { get; set; }

        // ReSharper disable once InconsistentNaming
        [Required]
        public DateTime Updated_At_Date { get; set; }

        public string Facility { get; set; }

        public string CommPkgNo { get; set; }

        public string McPkgNo { get; set; }

        public string PoNo { get; set; }

        public string CallOffNo { get; set; }

        // ReSharper disable once InconsistentNaming
        public DateTime? Signed_At_Date { get; set; }

        // ReSharper disable once InconsistentNaming
        public DateTime? Verified_At_Date { get; set; }

        // ReSharper disable once InconsistentNaming
        public DateTime? Fat_Planned_At_Date { get; set; }
    }
}
