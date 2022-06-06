using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using free_azure.api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace free_azure.api
{
    public class Events
    {
        private readonly FreeAzureContext dbContext;

        public Events(FreeAzureContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [FunctionName("Events")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                if (req.Method == HttpMethods.Post)
                {

                    var postEvent = await JsonSerializer.DeserializeAsync<Event>(req.Body);
                    // postEvent.PartitionKey = "1";
                    await this.dbContext.Events.AddAsync(postEvent);
                    await this.dbContext.SaveChangesAsync();
                    return new OkResult();
                }
                else
                {
                    log.LogInformation("try to create a list of events now.");
                    var result = this.dbContext.Events.ToListAsync();
                    return new OkObjectResult("result");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
