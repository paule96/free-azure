using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using free_azure.api.Models;

namespace free_azure.api
{
    public class Init
    {
        private readonly FreeAzureContext dbContext;

        public Init(FreeAzureContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [FunctionName("Init")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                log.LogInformation("Init db.");
                await this.dbContext.Database.EnsureCreatedAsync();
                return new OkResult();    
            }
            catch (System.Exception ex)
            {
                
                throw ex;
            }
            
        }
    }
}
