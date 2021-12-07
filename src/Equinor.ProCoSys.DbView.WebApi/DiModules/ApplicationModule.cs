using Equinor.ProCoSys.DbView.WebApi.Controllers.PbiCheckList;
using Equinor.ProCoSys.DbView.WebApi.Controllers.ThreeDEcoTag;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Equinor.ProCoSys.DbView.WebApi.Telemetry;

namespace Equinor.ProCoSys.DbView.WebApi.DIModules
{
    public static class ApplicationModule
    {
        public static void AddApplicationModules(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ApiOptions>(configuration.GetSection("ApiOptions"));

            services.AddHttpContextAccessor();
            services.AddHttpClient();

            // Hosted services

            // Transient - Created each time it is requested from the service container

            // Scoped - Created once per client request (connection)
            services.AddScoped<ITelemetryClient, ApplicationInsightsTelemetryClient>();
            services.AddScoped<IPbiCheckListRepository, PbiCheckListRepository>();
            services.AddScoped<ITagRepository, TagRepository>();

            // Singleton - Created the first time they are requested
        }
    }
}
