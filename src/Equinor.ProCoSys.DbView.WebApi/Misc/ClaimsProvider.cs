using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Equinor.ProCoSys.DbView.WebApi.Misc
{
    public class ClaimsProvider : IClaimsProvider
    {
        private readonly ClaimsPrincipal _principal;

        public ClaimsProvider(IHttpContextAccessor accessor) => _principal = accessor?.HttpContext?.User ?? new ClaimsPrincipal();

        public ClaimsPrincipal GetCurrentUser() => _principal;
    }
}
