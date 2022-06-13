namespace free_azure.shared;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public TimeSpan Duration => End - Start;
    public IEnumerable<Location> Locations { get; set; }
}
