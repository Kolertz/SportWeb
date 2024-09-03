using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        IUserSessionService userSessionService,
        ILogger<AccountController> logger,
        IPasswordCryptor passwordCryptor,
        IAvatarService avatarService,
        IFileService fileService,
        IReCaptchaService reCaptchaService,
        IOptions<GoogleReCaptchaSettings> reCaptchaSettings) : Controller
    {
        [HttpGet]
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
        public async Task<ActionResult> Login(LoginViewModel form, string? returnUrl)
        {
            var context = Request.HttpContext;

            var isCaptchaValid = await reCaptchaService.IsCaptchaValid(form.ReCaptchaToken!);
            if (!isCaptchaValid)
            {
                TempData["Message"] = ($"Captcha validation failed., ReCaptchaToken: {form.ReCaptchaToken}");
                return View(form);
            }
            User? user = db.Users.FirstOrDefault(u => u.Email == form.Email);
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
                new(ClaimTypes.Name, user.Id.ToString() ?? "default"),
            };
            ClaimsIdentity claimsIdentity = new(claims, "Cookies");
            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            TempData["Message"] = "Logged in successfully!";

            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/");
        }

        [HttpGet]
        public ActionResult Register() => View();

        [HttpPost]
        [ValidateModelStateFilter]
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
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["Message"] = "Logged out successfully!";
            logger.LogInformation("User logged out");

            return Redirect("/");
        }

        [Route("profile/{id}")]
        public async Task<IActionResult> Profile(string id)
        {
            User? user = await userRepository.GetUserAsync(id, true);
            if (user == null)
            {
                return NotFound();
            }
            var isUserProfile = userSessionService.IsCurrentUser(int.Parse(id));
            var addedExercisesQuery = db.Exercises.Where(x => x.AuthorId == user.Id);
            var addedExercisesCount = await addedExercisesQuery.CountAsync();  // Считаем общее количество
            var addedExercises = await addedExercisesQuery
                .Take(4)  // Берем только 4 записи
                .ToListAsync();

            // Получаем публичные тренировки пользователя
            var addedWorkoutsQuery = db.Workouts.Where(x => x.AuthorId == user.Id && x.IsPublic);
            var addedWorkoutsCount = await addedWorkoutsQuery.CountAsync();  // Считаем общее количество
            var addedWorkouts = await addedWorkoutsQuery.Take(4).ToListAsync();

            var model = new ProfileViewModel
            {
                IsUserProfile = isUserProfile,
                Name = user.Name ?? "Anonymous",
                Avatar = avatarService.GetAvatarUrl(user.Avatar),
                Description = user.Description,
                Id = user.Id.ToString(),
                AddedExercises = addedExercises,
                AddedExercisesCount = addedExercisesCount,
                AddedWorkouts = addedWorkouts,
                AddedWorkoutsCount = addedWorkoutsCount
            };

            if (user.IsPublicFavourites)
            {
                var favouriteExercisesQuery = db.Users.Where(x => x.Id == user.Id).SelectMany(x => x.FavouriteExercises);
                var favouriteExercises = await favouriteExercisesQuery.Take(4).ToListAsync();
                var favouriteExercisesCount = await favouriteExercisesQuery.CountAsync();
                model.FavouriteExercises = favouriteExercises;
                model.FavouriteExercisesCount = favouriteExercisesCount;
            }

            return View(model);
        }

        [HttpGet]
        [Authorize]
        [Route("profile/edit")]
        public async Task<IActionResult> Edit()
        {
            var id = userSessionService.GetCurrentUserId();

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

        [HttpPost]
        [Authorize]
        [Route("profile/edit")]
        [ValidateModelStateFilter]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            var id = userSessionService.GetCurrentUserId();
            User? user = await userRepository.GetUserAsync(id, false);
            if (user == null)
            {
                logger.LogWarning($"User with id {id} not found.");
                return NotFound();
            }

            var fileUpload = model.FileUpload;
            if (fileUpload != null && fileUpload.Length > 0)
            {
                user.Avatar = avatarService.NewAvatarName(user);
                var filePath = avatarService.GetAvatarPath(user.Avatar);
                await fileService.UploadFile(fileUpload, filePath);
            }
            Log.VariableLog(logger, nameof(model.Name), model.Name!);
            Log.VariableLog(logger, nameof(user.Name), user.Name!);
            user.Name = model.Name;
            user.Description = model.Description ?? "No description";
            user.IsPublicFavourites = model.IsPublicFavourites;

            if (model.Password != null)
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
                HttpContext.Session.Set("User", user);
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
        public async Task<IActionResult> Delete()
        {
            var userId = userSessionService.GetCurrentUserId();

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