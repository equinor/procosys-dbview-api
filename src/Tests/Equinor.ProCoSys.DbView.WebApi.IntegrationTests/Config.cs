using System;
using Microsoft.Extensions.Configuration;

namespace Equinor.ProCoSys.DbView.WebApi.IntegrationTests
{
    internal class Config
    {
        private static Config instance;
        private IConfigurationRoot _config;

        private Config()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            Console.WriteLine($"Running tests in {environment} environment");
            var optional = true;
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", optional)
                .AddUserSecrets<Config>(true)
                .Build();
        }

        private static Config Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Config();
                }
                return instance;
            }
        }

        public static string TestClientId(string clientConfigKey) => Instance._config[$"{clientConfigKey}:ClientId"];
        public static string TestClientSecret(string clientConfigKey) => Instance._config[$"{clientConfigKey}:ClientSecret"];
        public static string Authority => Instance._config["Authority"];
        public static string WebApiScope => Instance._config["WebApiScope"];
        public static string ApplicationUrl => Instance._config["ApplicationUrl"];
        
        public static string InstCodeUnderTest_Large => Instance._config["InstCodeUnderTest_Large"];
        public static string InstCodeUnderTest_Small => Instance._config["InstCodeUnderTest_Small"];
        public static string InstCodeUnderTest_Static => Instance._config["InstCodeUnderTest_Static"];

        public static string RandomInstCodeUnderTest
        {
            get
            {
                var codes = Instance._config["InstCodesUnderTest"].Split(';', StringSplitOptions.RemoveEmptyEntries);
                var idx = new Random().Next(codes.Length);
                return codes[idx];
            }
        }
    }
}
