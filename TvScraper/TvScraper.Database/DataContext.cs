using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TvScraper.Database.Model;

namespace TvScraper.Database
{
    public class DataContext : DbContext
    {
        public DataContext()
        {
             
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlServer("Server=.;Database=TvScraper;Trusted_Connection=true;TrustServerCertificate=true");
        }

        public DbSet<Show> Shows { get;set; }   
        public DbSet<CastMember> CastMembers { get;set; }   
        public DbSet<Actor> Actors { get;set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Show>().ToTable("Shows");
            modelBuilder.Entity<Show>()
                .HasMany(s => s.Cast)
                .WithOne(c => c.Show)
                .HasForeignKey(c => c.ActorId);


            modelBuilder.Entity<Actor>().ToTable("Actors");
            modelBuilder.Entity<Actor>()
                .HasMany(a => a.Shows)
                .WithOne(c => c.Actor)
                .HasForeignKey(c => c.ActorId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
