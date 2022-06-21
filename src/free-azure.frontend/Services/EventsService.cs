using System.Net.Http.Json;
using System.Text.Json;
using free_azure.shared;

namespace free_azure.frontend.Services
{
    public class EventsService
    {
        private readonly HttpClient client;
        private JsonSerializerOptions options = new JsonSerializerOptions()
                    {
                        PropertyNameCaseInsensitive = true
                    };

        public EventsService(HttpClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<Event>> GetEvents()
        {
            var result = await client.GetAsync("events");
            result.EnsureSuccessStatusCode();
            return System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Event>>(await result.Content.ReadAsStreamAsync(), options);
        }

        public async Task<bool> TestEvent(Event eventToTest)
        {
            
            var result = await client.PostAsJsonAsync("testevent", eventToTest, options);
            if(result.StatusCode == System.Net.HttpStatusCode.Conflict){
                return false;
            }
            result.EnsureSuccessStatusCode();
            return true;
        }

        public async Task CreateEvent(Event eventToCreate){
            var result = await client.PostAsJsonAsync("events", eventToCreate, options);
            result.EnsureSuccessStatusCode();
        }
    }
}