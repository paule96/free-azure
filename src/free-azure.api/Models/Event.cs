using System;
using System.Collections.Generic;

namespace free_azure.api.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        // public TimeSpan Duration => End - Start;
        // public IEnumerable<Location> Locations { get; set; }
        // public string PartitionKey { get; set; }
    }
}