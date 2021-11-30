namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests.Misc
{
    public class HeartbeatModel
    {
        public bool IsAlive { get; set; }
        public string TimeStamp { get; set; }
        public bool IsDbAlive { get; set; }
        public string DbTimeStamp { get; set; }
    }
}
