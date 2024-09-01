using System;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
namespace SportWeb.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; } = "Anonymous";
        public required string Email { get; set; }
        public required string Password { get; set; }
        public string Description { get; set; } = "No Description";
        public string Avatar { get; set; } = "avatar.png";
        public bool IsPublicFavourites { get; set; } = false;
        public ICollection<Exercise> Exercises { get; set; } = [];
        public ICollection<Exercise> FavouriteExercises { get; set; } = [];
        public ICollection<Workout> Workouts { get; set; } = [];
    }
}
