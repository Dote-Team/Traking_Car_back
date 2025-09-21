using Microsoft.EntityFrameworkCore;
using TrakingCar.Models;

namespace TrakingCar.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Car> Cars { get; set; }
        public DbSet<Ownership> Ownerships { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<LogEntry> LogEntries { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();
            // ------------------------------------------------------
            // Ownership ↔ Location (One-to-Many)
            // ------------------------------------------------------
            modelBuilder.Entity<Ownership>()
                .HasOne(a => a.Location)
                .WithMany(l => l.Ownerships)
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------
            // Location ↔ Car (One-to-Many)
            // ------------------------------------------------------
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Location)
                .WithMany(l => l.Cars)
                .HasForeignKey(c => c.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------
            // Location ↔ Attachment (One-to-Many)
            // ------------------------------------------------------
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Location)
                .WithMany(l => l.Attachments)
                .HasForeignKey(a => a.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------
            // Car ↔ Attachment (One-to-Many)
            // ------------------------------------------------------
            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Car)
                .WithMany(c => c.Attachments)
                .HasForeignKey(a => a.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------
            // Ownership ↔ Car (One-to-Many)
            // ------------------------------------------------------
            modelBuilder.Entity<Car>()
                .HasOne(c => c.Ownership)
                .WithMany(o => o.Cars)
                .HasForeignKey(c => c.OwnershipId)
                .OnDelete(DeleteBehavior.Restrict);

            // ------------------------------------------------------
            // الاسماء الخاصة بالجداول
            // ------------------------------------------------------
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Location>().ToTable("Location");
            modelBuilder.Entity<Car>().ToTable("Car");
            modelBuilder.Entity<Ownership>().ToTable("OwnerShip");
            modelBuilder.Entity<Attachment>().ToTable("Attachment");
            modelBuilder.Entity<LogEntry>().ToTable("LogEntries"); 

        }
    }
}
