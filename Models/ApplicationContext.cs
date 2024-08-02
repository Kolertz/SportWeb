using Microsoft.EntityFrameworkCore;
using SportWeb.Models.Entities;
namespace SportWeb.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            //Database.EnsureCreated();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Exercise>().HasMany(x => x.Categories).WithMany(x => x.Exercises);
            modelBuilder.Entity<User>().HasAlternateKey(x => x.Email);
            modelBuilder.Entity<Exercise>().HasOne(x => x.User).WithMany(x => x.Exercises).HasForeignKey(x => x.AuthorId);
            base.OnModelCreating(modelBuilder);
        }
    }
}
