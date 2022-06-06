using Microsoft.EntityFrameworkCore;

namespace free_azure.api.Models
{
    public class FreeAzureContext : DbContext
    {
        public FreeAzureContext(DbContextOptions<FreeAzureContext> options) : base(options)
        {

        }
        public DbSet<Event> Events { get; set; }
        public DbSet<Location> Locations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultContainer("Events");
            modelBuilder.Entity<Event>()
                .ToContainer("Events");
            modelBuilder.Entity<Event>()
                .HasNoDiscriminator();
            modelBuilder.Entity<Event>()
                .HasPartitionKey(e => e.PartitionKey);
            modelBuilder.Entity<Event>()
                .UseETagConcurrency();
            modelBuilder.Entity<Event>()
                .OwnsMany(e => e.Locations);

        }
    }
}