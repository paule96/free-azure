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
using System.Linq;
using Microsoft.Azure.Cosmos;

namespace free_azure.api
{
    public class TestEvent
    {
        private readonly FreeAzureContext dbContext;
        private readonly ILogger<TestEvent> logger;

        public TestEvent(FreeAzureContext dbContext, ILogger<TestEvent> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }
        [FunctionName("TestEvent")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            try
            {
                var postEvent = await JsonSerializer.DeserializeAsync<Event>(req.Body);
                if (postEvent.Locations.Any(l => l.Virtual == false))
                {
                    var query = this.dbContext.Events.FromSqlRaw(@$"
                    SELECT
                        *
                    FROM Events as e
                    Where
                        (
                            (
                                -- the start of our event is in an exsisting event
                                e[""Start""] <= '{postEvent.Start.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")}' AND
                                e[""End""] >= '{postEvent.Start.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")}'
                            ) OR
                            (
                                -- our end is to late, so we are overlapping with an other event
                                e[""Start""] >= '{postEvent.Start.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")}' AND
                                e[""Start""] <= '{postEvent.End.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")}'
                            )
                        ) AND
                        (
                            Select Count(1) as Count
                            From locations in e[""Locations""]
                            Where
                                locations[""Virtual""] = false And
                                Array_Contains([""{string.Join("\",\"", postEvent.Locations.Where(l => l.Virtual == false).Select(e => e.Name))}""], locations[""Name""]) = true
                        )[""Count""] > 0
                    ");


                    var alreadyBooked = await query
                        .LongCountAsync() > 0;
                    if (alreadyBooked)
                    {
                        var events = await query
                            .Select(e => $"'{e.Name}', {e.Id}")
                            .ToListAsync();
                        var messageString = string.Join(" | ", events);
                        return new ConflictObjectResult(
                            $"The event is not possible, because it is in the same time like: {messageString}"
                        );
                    }
                }
                return new OkResult();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
