namespace Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag
{
    public class Tag
    {
        public string TagNo { get; set; }
        public string Project { get; set; }
        public int PunchCount { get; set; }
        public string CommPkgNo { get; set; }
        public string CommPkgDesc { get; set; }
        public string McPkgNo { get; set; }
        public string McPkgDesc { get; set; }
        public string Priority { get; set; }
        public string Phase { get; set; }
        public string Rfcc { get; set; }
        public string Rfoc { get; set; }
        public string Responsible { get; set; }
        public string Status { get; set; }
        public int? CommPkgId { get; set; }
    }
}
