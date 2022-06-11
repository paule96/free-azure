namespace free_azure.api.Models
{
    public class Location
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Virtual { get; set; } = false;
    }
}