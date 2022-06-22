using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using free_azure.api.Models;
using System.Net.Http;
using System;

[assembly: FunctionsStartup(typeof(free_azure.api.Startup))]
namespace free_azure.api
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            // this is hopefulle a static key for the storage account emulator
            builder.Services.AddCosmos<FreeAzureContext>(connectionString, "something", o =>
            {
                o.HttpClientFactory(() =>
                    {
                        HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                        {
                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        };

                        return new HttpClient(httpMessageHandler);
                    }
                );
                o.ConnectionMode(Microsoft.Azure.Cosmos.ConnectionMode.Gateway);
            });
        }
    }
}