using System.Security.Claims;

namespace Equinor.ProCoSys.DbView.WebApi.Misc
{
    public interface IClaimsProvider
    {
        ClaimsPrincipal GetCurrentUser();
    }
}
