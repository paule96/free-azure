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

        public TestEvent(FreeAzureContext dbContext)
        {
            this.dbContext = dbContext;
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
                    /*
                        SELECT
    Count(1) as Count
    -- e["Start"] <= '2022-06-11T19:01:55.0+00:00' as greaterOrEqualStart,
    -- e["End"] >= '2022-06-11T20:01:55.0+00:00' as lessOrEqualEnd,
    -- (
    --     Select Count(1) as Count
    --     From locations in e["Locations"]
    --     Where
    --         locations["Virtual"] = false And
    --         Array_Contains(["test", "test2"], locations["Name"]) = false
    -- )["Count"] as locationCount,
    -- e as object
FROM Events as e
Where
    e["Start"] <= '2022-06-11T19:01:55.0+00:00' And
    e["End"] >= '2022-06-11T21:01:55.0+00:00' And
    (
        Select Count(1) as Count
        From locations in e["Locations"]
        Where
            locations["Virtual"] = false And
            Array_Contains(["test", "test2", "Super cool"], locations["Name"]) = true
    )["Count"] > 0
                    */
                    // var query = new QueryDefinition(@$"
                    //     Select
                    //     From Events as
                    //     Where
                    //         Start >= '{postEvent.Start}' AND
                    //         End <= '{postEvent.End}' AND
                    //         ARRAY_CONTAINS(Locations,)

                    // ");
                    // var cosmosClient = dbContext.Database.GetCosmosClient();
                    // var container = cosmosClient.GetDatabaseQueryIterator<Event>("");
                    // var realLocationIds = postEvent.Locations
                    //     .Where(l => l.Virtual == false)
                    //     .Select(l => l.Id)
                    //     .ToList();
                    var query = this.dbContext.Events.FromSqlRaw(@$"
                    SELECT
                        *
                    FROM Events as e
                    Where
                        e[""Start""] <= '2022-06-11T19:01:55.0+00:00' And
                        e[""End""] >= '2022-06-11T21:01:55.0+00:00' And
                        (
                            Select Count(1) as Count
                            From locations in e[""Locations""]
                            Where
                                locations[""Virtual""] = false And
                                Array_Contains([""test"", ""test2"", ""Super cool""], locations[""Name""]) = true
                        )[""Count""] > 0
                    ");
                    // var query = this.dbContext.Events
                    //     .Where(e =>
                    //         e.Start >= postEvent.Start &&
                    //         e.End <= postEvent.End
                    //         e.Locations
                    //             .Where(l => l.Virtual == false)
                    //             .Count() > 0
                    //             .Any(l =>
                    //                 postEvent.Locations.Select(pl => pl.Name)
                    //                     .Contains(l.Name)
                    //             )
                    //     );
                    var alreadyBooked = await query
                        .LongCountAsync() > 0;
                    if (alreadyBooked)
                    {
                        var events = await query
                            .Select(e => $"{e.Name}, {e.Id}")
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
