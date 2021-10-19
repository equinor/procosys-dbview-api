using System;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.PbiCheckList
{
    public class CheckListInstance
    {
        // ReSharper disable once InconsistentNaming
        public long CheckList_Id { get; set; }
        public string Projectschema { get; set; }
        public string Project { get; set; }
        public string TagNo { get; set; }
        // ReSharper disable once InconsistentNaming
        public string Tag_Category { get; set; }
        public string FormularType { get; set; }
        public string FormularGroup { get; set; }
        public string FormularDiscipline { get; set; }
        public string Responsible { get; set; }
        public string Status { get; set; }
        // ReSharper disable once InconsistentNaming
        public DateTime Created_At_Date { get; set; }
        // ReSharper disable once InconsistentNaming
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
