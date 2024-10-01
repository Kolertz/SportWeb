using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SportWeb.Extensions;
using SportWeb.Filters;
using SportWeb.Models;
using SportWeb.Models.Entities;
using SportWeb.Services;
using System.Security.Claims;

namespace SportWeb.Controllers
{
    public class AccountController(
        ApplicationContext db,
        IUserRepository userRepository,
        IUserCacheService userCacheService,
        ILogger<AccountController> logger,
        IPasswordCryptorService passwordCryptor,
        IAvatarService avatarService,
        IReCaptchaService reCaptchaService,
        IOptions<GoogleReCaptchaSettings> reCaptchaSettings,
        IFileUploadFacadeService fileUploadFacadeService) : Controller
    {
        
        [HttpGet]
        [OutputCache(PolicyName = "NoCache")]
        public ActionResult Login()
        {
            var model = new LoginViewModel
            {
                SiteKey = reCaptchaSettings.Value.SiteKey // Можно также получить ключ из конфигурации
            };

            return View(model);
        }

        [HttpPost]
        [EnableRateLimiting("LoginRateLimit")]
        [ValidateModelStateFilter]
        [OutputCache(PolicyName = "NoCache")]
        public async Task<ActionResult> Login(LoginViewModel form, string? returnUrl)
        {
            var context = Request.HttpContext;

            var isCaptchaValid = await reCaptchaService.IsCaptchaValid(form.ReCaptchaToken!);
            if (!isCaptchaValid)
            {
                TempData["Message"] = ($"Captcha validation failed., ReCaptchaToken: {form.ReCaptchaToken}");
                return View(form);
            }
            //User? user = db.Users.FirstOrDefault(u => u.Email == form.Email);
            User? user = await userRepository.GetUserAsync(form.Email, true);
            if (user == null)
            {
                ViewBag.Message = "User with such email is not found";
                return View(form);
            }
            if (!passwordCryptor.Verify(form.Password!, user.Password!))
            {
                ViewBag.Message = "Wrong password";
                return View(form);
            }
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Id.ToString() ?? "default")
            };
            ClaimsIdentity claimsIdentity = new(claims, "Cookies");
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            TempData["Message"] = "Logged in successfully!";

            if (returnUrl is not null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/");
        }

        [HttpGet]
        [OutputCache(PolicyName = "NoUniqueContent")]
        public ActionResult Register() => View();

        [HttpPost]
        [ValidateModelStateFilter]
        [OutputCache(PolicyName = "NoCache")]
        public ActionResult Register(RegisterViewModel form)
        {
            if (userRepository.IsUserExistsByEmail(form.Email).Result)
            {
                TempData["Message"] = "User with such email already exists.";
                return RedirectToAction(nameof(Register));
            }
            userRepository.AddUserAsync(form.Name, form.Email, form.Password);

            TempData["Message"] = "Registered successfully!";
            return Redirect("/");
        }

        [OutputCache(PolicyName = "NoCache")]
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["Message"] = "Logged out successfully!";
            logger.LogInformation("User logged out");

            return Redirect("/");
        }

        [HttpGet("profile/{id}")]
        [OutputCache(PolicyName = "UserDataUnique")]
        public async Task<IActionResult> Profile(string id)
        {
            // Получаем пользователя
            User? user = await userRepository.GetUserAsync(id, true);
            if (user == null)
            {
                return NotFound();
            }

            // Проверяем, является ли профиль текущего пользователя
            var isUserProfile = userCacheService.IsCurrentUser(int.Parse(id));

            // Запрос для добавленных упражнений пользователя
            var addedExercisesQuery = db.Exercises
                .Where(x => x.AuthorId == user.Id)
                .Select(x => new { x.Id, x.Name, x.Description }); // Загрузить только необходимые поля

            var addedExercises = await addedExercisesQuery
                .Take(4)
                .ToListAsync();
            var addedExercisesCount = await addedExercisesQuery.CountAsync();

            // Запрос для добавленных публичных тренировок пользователя
            var addedWorkoutsQuery = db.Workouts
                .Where(x => x.AuthorId == user.Id && x.IsPublic)
                .Select(x => new { x.Id, x.Name, x.Description }); // Загрузить только необходимые поля

            var addedWorkouts = await addedWorkoutsQuery
                .Take(4)
                .ToListAsync();
            var addedWorkoutsCount = await addedWorkoutsQuery.CountAsync();

            // Заполнение модели
            var model = new ProfileViewModel
            {
                IsUserProfile = isUserProfile,
                Name = user.Name ?? "Anonymous",
                Avatar = avatarService.GetAvatarUrl(user.Avatar),
                Description = user.Description,
                Id = user.Id.ToString(),
                AddedExercises = addedExercises.Select(x => new ExerciseViewModel { Id = x.Id, Name = x.Name }).ToList(),
                AddedExercisesCount = addedExercisesCount,
                AddedWorkouts = addedWorkouts.Select(x => new WorkoutViewModel { Id = x.Id, Name = x.Name }).ToList(),
                AddedWorkoutsCount = addedWorkoutsCount
            };

            // Если избранное публично, загружаем избранные упражнения
            if (user.IsPublicFavourites)
            {
                var favouriteExercisesQuery = db.Users
                    .Where(x => x.Id == user.Id)
                    .SelectMany(x => x.FavouriteExercises)
                    .Select(x => new { x.Id, x.Name }); // Загрузить только необходимые поля

                var favouriteExercises = await favouriteExercisesQuery
                    .Take(4)
                    .ToListAsync();
                var favouriteExercisesCount = await favouriteExercisesQuery.CountAsync();

                model.FavouriteExercises = favouriteExercises.Select(x => new ExerciseViewModel { Id = x.Id, Name = x.Name }).ToList();
                model.FavouriteExercisesCount = favouriteExercisesCount;
            }

            // Возвращаем модель во View
            return View(model);
        }

        [HttpGet("profile/edit")]
        [Authorize]
        [OutputCache(PolicyName = "UserData")]
        public async Task<IActionResult> Edit()
        {
            var id = userCacheService.GetCurrentUserId();
            User? user = await userRepository.GetUserAsync(id, true);

            if (user == null)
            {
                return NotFound();
            }
            EditProfileViewModel model = new()
            {
                Name = user.Name,
                Password = user.Password,
                Avatar = avatarService.GetAvatarUrl(user.Avatar),
                Description = user.Description,
                IsPublicFavourites = user.IsPublicFavourites,
            };
            return View(model);
        }

        [HttpPost("profile/edit")]
        [Authorize]
        [ValidateModelStateFilter]
        [OutputCache(PolicyName = "NoCache")]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var id = userCacheService.GetCurrentUserId();
            User? user = await userRepository.GetUserAsync(id, false);
            if (user == null)
            {
                logger.LogWarning($"User with id {id} not found.");
                return NotFound();
            }
            
            var userAvatar = await fileUploadFacadeService.UploadFile(model.FileUpload, user.Avatar, user.Id);
            user.Avatar = userAvatar ?? user.Avatar;

            Log.VariableLog(logger, nameof(model.Name), model.Name!);
            Log.VariableLog(logger, nameof(user.Name), user.Name!);
            user.Name = model.Name;
            user.Description = model.Description ?? "No description";
            user.IsPublicFavourites = model.IsPublicFavourites;

            if (model.Password is not null)
            {
                if (model.Password == model.ConfirmPassword && model.Password.Length > 5)
                {
                    user.Password = passwordCryptor.Hash(model.Password);
                }
                else
                {
                    TempData["Message"] = "Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character";
                    return View(model);
                }
            }

            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync();
                logger.LogInformation("Changes were made to the database");
                await userCacheService.RemoveUserFromCacheAsync(user.Id);
            }
            else
            {
                logger.LogInformation("No changes were made to the database");
            }
            TempData["Message"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile), new { id });
        }

        [HttpPost]
        [Authorize]
        [OutputCache(PolicyName = "NoCache")]
        public async Task<IActionResult> Delete()
        {
            var userId = userCacheService.GetCurrentUserId();

            if (await userRepository.RemoveUserAsync(userId))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                TempData["Message"] = "Something went wrong.";
                return Redirect("/");
            }
            TempData["Message"] = "Account deleted successfully!";
            return Redirect("/");
        }
    }
}