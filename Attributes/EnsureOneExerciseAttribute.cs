using SportWeb.Models.Entities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;

namespace SportWeb.Attributes
{
    public class EnsureOneExerciseAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is ICollection<object> items)
            {
                // Отладочная информация: какие элементы находятся в items
                Console.WriteLine("EnsureOneExercise - Items count: " + items.Count);
                foreach (var item in items)
                {
                    Console.WriteLine("Item type: " + item.GetType().Name);
                }

                // Проверка наличия хотя бы одного упражнения
                if (items.Any(e => e is WorkoutExercise || e is Superset superset && superset.WorkoutExercises.Count != 0))
                {
                    return ValidationResult.Success!;
                }
            }
            return new ValidationResult(ErrorMessage ?? "You must add at least one exercise.");
        }
    }
}
