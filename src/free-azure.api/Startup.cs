using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using free_azure.api.Models;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(free_azure.api.Startup))]
namespace free_azure.api
{
    class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // string connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            // this is hopefulle a static key for the storage account emulator
            string key = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

            string connectionString = $"AccountEndpoint=https://cosmosdb:8081/;AccountKey={key};";
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