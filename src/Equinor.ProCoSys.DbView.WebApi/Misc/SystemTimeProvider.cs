using System;

namespace Equinor.ProCoSys.DbView.WebApi.Misc
{
    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}
