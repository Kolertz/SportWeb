using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SportWeb.Models.Entities;
using Microsoft.AspNetCore.Hosting;

namespace SportWeb.Models
{
    public class ApplicationContext(DbContextOptions<ApplicationContext> options, IWebHostEnvironment env) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Workout> Workouts { get; set; } = null!;
        public DbSet<WorkoutExercise> WorkoutExercises { get; set; } = null!;
        public DbSet<Superset> Supersets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Exercise>().HasMany(x => x.Categories).WithMany(x => x.Exercises);
            modelBuilder.Entity<Exercise>().HasOne(x => x.User).WithMany(x => x.Exercises).HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Exercise>().HasIndex(x => x.AuthorId);

            modelBuilder.Entity<User>().HasMany(x => x.FavouriteExercises).WithMany(x => x.UsersWhoFavourited).UsingEntity(e => e.ToTable("FavouriteExercises"));
            modelBuilder.Entity<User>().HasAlternateKey(x => x.Email);
            
            modelBuilder.Entity<Workout>().HasOne(x => x.User).WithMany(x => x.Workouts).HasForeignKey(x => x.AuthorId).OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Workout>().HasMany(x => x.Supersets).WithOne(x => x.Workout).HasForeignKey(x => x.WorkoutId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Workout>().HasIndex(x => x.AuthorId);
            modelBuilder.Entity<Workout>().HasIndex(x => x.IsPublic);

            modelBuilder.Entity<WorkoutExercise>().HasKey(we => new { we.WorkoutId, we.ExerciseId }); // Составной первичный ключ
            modelBuilder.Entity<WorkoutExercise>().HasOne(we => we.Workout).WithMany(w => w.WorkoutExercises).HasForeignKey(we => we.WorkoutId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WorkoutExercise>().HasOne(we => we.Exercise).WithMany(e => e.WorkoutExercises).HasForeignKey(we => we.ExerciseId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<WorkoutExercise>().HasOne(we => we.Superset).WithMany(s => s.WorkoutExercises).HasForeignKey(s => s.SupersetId).OnDelete(DeleteBehavior.NoAction);

            
            base.OnModelCreating(modelBuilder);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (env.IsDevelopment())
            {
                optionsBuilder.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
                optionsBuilder.EnableSensitiveDataLogging();
                optionsBuilder.EnableDetailedErrors();
            }
        }
    }
}
