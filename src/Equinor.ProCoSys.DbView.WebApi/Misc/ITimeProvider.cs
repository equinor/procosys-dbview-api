using System;

namespace Equinor.ProCoSys.DbView.WebApi.Misc
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }
}
