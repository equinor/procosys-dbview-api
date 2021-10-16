using System;

namespace Equinor.ProCoSys.DbView.WebApi.Misc
{
    public interface ICurrentUserSetter
    {
        void SetCurrentUserOid(Guid oid);
    }
}
