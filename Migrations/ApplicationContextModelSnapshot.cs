﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SportWeb.Models;

#nullable disable

namespace SportWeb.Migrations
{
    [DbContext(typeof(ApplicationContext))]
    partial class ApplicationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CategoryExercise", b =>
                {
                    b.Property<int>("CategoriesId")
                        .HasColumnType("int");

                    b.Property<int>("ExercisesId")
                        .HasColumnType("int");

                    b.HasKey("CategoriesId", "ExercisesId");

                    b.HasIndex("ExercisesId");

                    b.ToTable("CategoryExercise");
                });

            modelBuilder.Entity("ExerciseUser", b =>
                {
                    b.Property<int>("FavouriteExercisesId")
                        .HasColumnType("int");

                    b.Property<int>("UsersWhoFavouritedId")
                        .HasColumnType("int");

                    b.HasKey("FavouriteExercisesId", "UsersWhoFavouritedId");

                    b.HasIndex("UsersWhoFavouritedId");

                    b.ToTable("FavouriteExercises", (string)null);
                });

            modelBuilder.Entity("SportWeb.Models.Entities.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.Exercise", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AuthorId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PictureUrl")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("State")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.ToTable("Exercises");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.Superset", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("Position")
                        .HasColumnType("int");

                    b.Property<int>("WorkoutId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("WorkoutId");

                    b.ToTable("Supersets");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Avatar")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<bool>("IsPublicFavourites")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasAlternateKey("Email");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.Workout", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AuthorId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AuthorId");

                    b.HasIndex("IsPublic");

                    b.ToTable("Workouts");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.WorkoutExercise", b =>
                {
                    b.Property<int>("WorkoutId")
                        .HasColumnType("int");

                    b.Property<int>("ExerciseId")
                        .HasColumnType("int");

                    b.Property<int>("Position")
                        .HasColumnType("int");

                    b.Property<int?>("SupersetId")
                        .HasColumnType("int");

                    b.HasKey("WorkoutId", "ExerciseId");

                    b.HasIndex("ExerciseId");

                    b.HasIndex("SupersetId");

                    b.ToTable("WorkoutExercises");
                });

            modelBuilder.Entity("CategoryExercise", b =>
                {
                    b.HasOne("SportWeb.Models.Entities.Category", null)
                        .WithMany()
                        .HasForeignKey("CategoriesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SportWeb.Models.Entities.Exercise", null)
                        .WithMany()
                        .HasForeignKey("ExercisesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ExerciseUser", b =>
                {
                    b.HasOne("SportWeb.Models.Entities.Exercise", null)
                        .WithMany()
                        .HasForeignKey("FavouriteExercisesId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SportWeb.Models.Entities.User", null)
                        .WithMany()
                        .HasForeignKey("UsersWhoFavouritedId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("SportWeb.Models.Entities.Exercise", b =>
                {
                    b.HasOne("SportWeb.Models.Entities.User", "User")
                        .WithMany("Exercises")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("User");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.Superset", b =>
                {
                    b.HasOne("SportWeb.Models.Entities.Workout", "Workout")
                        .WithMany("Supersets")
                        .HasForeignKey("WorkoutId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Workout");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.Workout", b =>
                {
                    b.HasOne("SportWeb.Models.Entities.User", "User")
                        .WithMany("Workouts")
                        .HasForeignKey("AuthorId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("User");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.WorkoutExercise", b =>
                {
                    b.HasOne("SportWeb.Models.Entities.Exercise", "Exercise")
                        .WithMany("WorkoutExercises")
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SportWeb.Models.Entities.Superset", "Superset")
                        .WithMany("WorkoutExercises")
                        .HasForeignKey("SupersetId")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("SportWeb.Models.Entities.Workout", "Workout")
                        .WithMany("WorkoutExercises")
                        .HasForeignKey("WorkoutId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Exercise");

                    b.Navigation("Superset");

                    b.Navigation("Workout");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.Exercise", b =>
                {
                    b.Navigation("WorkoutExercises");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.Superset", b =>
                {
                    b.Navigation("WorkoutExercises");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.User", b =>
                {
                    b.Navigation("Exercises");

                    b.Navigation("Workouts");
                });

            modelBuilder.Entity("SportWeb.Models.Entities.Workout", b =>
                {
                    b.Navigation("Supersets");

                    b.Navigation("WorkoutExercises");
                });
#pragma warning restore 612, 618
        }
    }
}
