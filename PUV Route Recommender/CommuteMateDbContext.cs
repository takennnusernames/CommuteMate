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
        public DbSet<OfflinePath> OfflinePaths { get; set; }
        public DbSet<OfflineStep> Steps { get; set; }
        public DbSet<Summary> Summaries { get; set; }

        //Join Tables
        public DbSet<RouteStreet> RouteStreets { get; set; }
        public DbSet<PathStep> PathSteps { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "Commute_Mate_DataBase.db3");
            optionsBuilder.UseSqlite($"Data Source={databasePath}");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Data tables
            modelBuilder.Entity<Route>().HasKey(r => r.RouteId);
            modelBuilder.Entity<Street>().HasKey(s => s.StreetId);
            modelBuilder.Entity<OfflinePath>().HasKey(s => s.Id);
            modelBuilder.Entity<OfflineStep>().HasKey(s => s.Id);
            modelBuilder.Entity<Summary>().HasKey(s => s.Id);

            //Join Tables
            modelBuilder.Entity<RouteStreet>().HasKey(s => s.RelationId);
            //many-to-many
            modelBuilder.Entity<RouteStreet>()
                .HasOne(rs => rs.Route)
                .WithMany(s => s.RouteStreets)
                .HasForeignKey(rs => rs.RouteOsmId)
                .HasPrincipalKey(r => r.OsmId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<RouteStreet>()
                .HasOne(rs => rs.Street)
                .WithMany(s => s.RouteStreets)
                .HasForeignKey(rs => rs.StreetOsmId)
                .HasPrincipalKey(r => r.OsmId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<PathStep>().HasKey(s => s.Id);
            //one-to-many
            modelBuilder.Entity<PathStep>()
                .HasOne(ps => ps.Path)
                .WithMany(p => p.PathSteps)
                .HasForeignKey(ps => ps.PathId)
                .HasPrincipalKey(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}