using Microsoft.EntityFrameworkCore;
using ScoreBoard.Shared;

namespace ScoreBoard.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<GameConsoleInfo> GameConsoles { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<GameConsoleInfo>().HasIndex(i => i.Number).IsUnique();
            base.OnModelCreating(modelBuilder);
        }
    }
}
