using System.ComponentModel.DataAnnotations;
using Cww.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Cww.Core.Database
{
    public class CwwDbContext : DbContext
    {
        public CwwDbContext(DbContextOptions<CwwDbContext> dbContextOptions)
            : base(dbContextOptions)
        {
        }

        public DbSet<Track> Tracks { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Track>().Ignore(t => t.UserPlayCounts);
        }
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string Username { get; set; }
    }
}
