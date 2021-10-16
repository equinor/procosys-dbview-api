using Equinor.ProCoSys.DbView.WebApi.Misc;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.DbView.WebApi.Authentication
{
    public class Authenticator : IBearerTokenSetter
    {
        private readonly ILogger<Authenticator> _logger;
        private string _requestToken;

        public Authenticator(ILogger<Authenticator> logger) => _logger = logger;

        public void SetBearerToken(string token) => _requestToken = token;
    }
}
