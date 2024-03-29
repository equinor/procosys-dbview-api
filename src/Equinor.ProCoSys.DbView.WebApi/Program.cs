﻿using System;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;

namespace Equinor.ProCoSys.DbView.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((_, config) =>
                {
                    var settings = config.Build();
                    var azConfig = settings.GetValue<bool>("UseAzureAppConfiguration");
                    if (azConfig)
                    {
                        config.AddAzureAppConfiguration(options =>
                        {
                            var connectionString = settings["ConnectionStrings:AppConfig"];
                            options.Connect(connectionString)
                                .ConfigureKeyVault(kv =>
                                {
                                    kv.SetCredential(new DefaultAzureCredential());
                                })
                                .Select(KeyFilter.Any)
                                .Select(KeyFilter.Any, settings["Azure:AppConfigLabelFilter"])
                                .ConfigureRefresh(refreshOptions =>
                                {
                                    refreshOptions.Register("Sentinel", true);
                                    refreshOptions.SetCacheExpiration(TimeSpan.FromMinutes(5));
                                });
                        });

                        //Download Oracle wallet file
                        var blobContainerClient = new BlobContainerClient(settings["WalletStorageAccountConnectionString"], settings["WalletContainerName"]);
                        var blobClient = blobContainerClient.GetBlobClient(settings["WalletBlobName"]);
                        var downloadPath = settings["WalletDownloadLocation"]; 
                        blobClient.DownloadTo(downloadPath);

                    }
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseIISIntegration();
                    webBuilder.UseStartup<Startup>();
                });
    }
}
