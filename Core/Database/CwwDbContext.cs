using System.ComponentModel.DataAnnotations;
using Cww.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Cww.Core.Database
{
    public class CwwDbContext : DbContext
    {
        public DbSet<Track> Tracks { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseMySql("server=localhost;database=library;user=cww;password=K7i8!mW{_9J>+)P9JxQE]_i0", b =>
            {
                b.MigrationsAssembly("Cww.Api");
            });
    }

    public class User
    {
        [Key]
        public int UserId { get; set; }

        public string Username { get; set; }
    }
}
