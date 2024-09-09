using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate
{
    public class CommuteMateDbContext : DbContext
    {
        public DbSet<Route> Routes { get; set; }
        public DbSet<Street> Streets { get; set; }
        public DbSet<RouteStreet> RouteStreets { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Commute_Mate_DataBase.db3");
            optionsBuilder.UseSqlite($"Data Source={databasePath}");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Route>().HasKey(r => r.RouteId);
            modelBuilder.Entity<Street>().HasKey(s => s.StreetId);
            modelBuilder.Entity<RouteStreet>().HasKey(s => s.RelationId);

            modelBuilder.Entity<RouteStreet>()
                .HasOne(rs => rs.Route)
                .WithMany(s => s.RouteStreets)
                .HasForeignKey(rs => rs.StreetId)
                .HasPrincipalKey(r => r.RouteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RouteStreet>()
                .HasOne(rs => rs.Street)
                .WithMany(s => s.RouteStreets)
                .HasForeignKey(rs => rs.StreetId)
                .HasPrincipalKey(r => r.StreetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Route>()
                .HasMany(r => r.Streets)
                .WithMany(s => s.Routes)
                .UsingEntity<Dictionary<string, object>>(
                    "RouteStreet",
                    r => r
                        .HasOne<Street>()
                        .WithMany()
                        .HasPrincipalKey(s => s.StreetId)
                        .HasForeignKey("StreetId"),
                    r => r
                        .HasOne<Route>()
                        .WithMany()
                        .HasPrincipalKey(r => r.RouteId)
                        .HasForeignKey("OsmId")
                );
        }
    }
}