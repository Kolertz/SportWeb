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
        public ICollection<Exercise>? Exercises { get; set; }
        public ICollection<Exercise>? FavoriteExercises { get; set; }
        public ICollection<Workout>? Workouts { get; set; }
        /*
        public User() { }
        public User(string? name, string email, string password)
        {
            Name = name;
            Email = email;
            Password = password;
        }
        */
    }
}
