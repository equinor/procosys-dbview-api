namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests
{
    public class Route
    {
        public class DbView
        {
            public class PbiCheckList
            {
                public static string Get => "/api/DbView/PbiCheckList";
                public static string Count => "/api/DbView/PbiCheckList/Count";
            }
        }
    }
}
