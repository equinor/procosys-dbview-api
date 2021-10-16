using System;

namespace Equinor.ProCoSys.DbView.WebApi.Misc
{
    public interface ICurrentUserProvider
    {
        Guid GetCurrentUserOid();
        bool HasCurrentUser();
    }
}
