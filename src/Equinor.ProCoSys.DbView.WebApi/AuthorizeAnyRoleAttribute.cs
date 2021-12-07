using Microsoft.AspNetCore.Authorization;

namespace Equinor.ProCoSys.DbView.WebApi
{
    public class AuthorizeAnyRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeAnyRoleAttribute()
        {
        }

        public AuthorizeAnyRoleAttribute(params string[] roles) => Roles = string.Join(",", roles);
    }
}
