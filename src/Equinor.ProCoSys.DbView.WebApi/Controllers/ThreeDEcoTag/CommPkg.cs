namespace Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag
{
    public partial class CommPkg
    {
        public int CommPkgId { get; set; }
        public string RFCCHandoverStatus
            => CertificateStatus(
                    McPkgsSentToCommissioning,
                    McPkgsAcceptedByCommissioning,
                    McPkgsRejectedByCommissioning);

        public string RFOCHandoverStatus
            => CertificateStatus(
                    McPkgsSentToOperation,
                    McPkgsAcceptedByOperation,
                    McPkgsRejectedByOperation);

        internal int McPkgCount { get; set; }
        
        internal int McPkgsSentToCommissioning { get; set; }
        internal int McPkgsRejectedByCommissioning { get; set; }
        internal int McPkgsAcceptedByCommissioning { get; set; }

        internal int McPkgsSentToOperation { get; set; }
        internal int McPkgsRejectedByOperation { get; set; }
        internal int McPkgsAcceptedByOperation { get; set; }

        private string CertificateStatus(int sentCount, int acceptedCount, int rejectedCount)
        {
            if (McPkgCount > 0 && McPkgCount == rejectedCount)
            {
                return "Rejected";
            }
            if (McPkgCount > 0 && acceptedCount > 0 && acceptedCount < McPkgCount)
            {
                return "Partly accepted";
            }
            if (McPkgCount > 0 && acceptedCount == McPkgCount)
            {
                return "Fully accepted";
            }
            if (sentCount > 0)
            {
                return "Sent";
            }
            if (rejectedCount > 0)
            {
                return "Rejected";
            }

            return "Not sent";
        }
    }
}
