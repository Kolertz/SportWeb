using Microsoft.AspNetCore.Mvc.Rendering;
using SportWeb.Models.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace SportWeb.Models
{
    public class ExerciseFilterModel
    {
        public SelectList Movements { get; set; } = new SelectList(new List<Category>(), "Id", "Name");
        public SelectList Equipments { get; set; } = new SelectList(new List<Category>(), "Id", "Name");
        public SelectList Tags { get; set; } = new SelectList(new List<Category>(), "Id", "Name");
        public SelectList Muscles { get; set; } = new SelectList(new List<Category>(), "Id", "Name");
        public string? Name { get; set; }
    }
}
