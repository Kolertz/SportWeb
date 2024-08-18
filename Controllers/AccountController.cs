using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SportWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.RegularExpressions;
using SportWeb.Services;
using Microsoft.Extensions.Logging;
using SportWeb.Models.Entities;

namespace SportWeb.Controllers
{
    public class AccountController(ApplicationContext context, ILogger<AccountController> logger, IUserService userService, IPasswordCryptor passwordCryptor, IAvatarService avatarService, IFileService fileService) : ControllerBase(context, logger, userService, fileService)
    {
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Login(User form, string? returnUrl)
        {
            var context = Request.HttpContext;
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
            TempData["Message"] = "Logged in successfully";
            
            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return Redirect("/");
        }
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel form)
        {
            if (ModelState.IsValid)
            {
                if (userService.IsUserExistsByEmail(form.Email).Result)
                {
                    TempData["Message"] = "User with such email already exists";
                    return RedirectToAction(nameof(Register));
                }
                userService.AddUserAcync(form.Name, form.Email, form.Password);

                TempData["Message"] = "Registered successfully";
                return Redirect("/");
            }
            else
            {
                return View(form);
            }
        }
        public async Task<ActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["Message"] = "Logged out successfully";
            logger.LogInformation("User logged out");

            return Redirect("/");
        }

        [Route("Profile/{id})")]
        public async Task<IActionResult> Profile(string id)
        {
            User? user = await userService.GetUserAsync(id, true);
            if (user == null)
            {
                return NotFound();
            }
            var isUserProfile = userService.IsCurrentUser(int.Parse(id));
            var addedExercises = db.Exercises.Where(x => x.AuthorId == user.Id).ToList();
            var addedExercisesCount = addedExercises.Count;
            addedExercises = addedExercises.Take(4).ToList();

            var addedWorkouts = db.Workouts.Where(x => x.AuthorId == user.Id && x.IsPublic).ToList();
            var addedWorkoutsCount = addedWorkouts.Count;
            addedWorkouts = addedWorkouts.Take(4).ToList();

            var model = new ProfileViewModel
            {
                IsUserProfile = isUserProfile,
                Name = user.Name,
                Avatar = avatarService.GetAvatarUrl(user.Avatar),
                Description = user.Description,
                Id = user.Id.ToString(),
                AddedExercises = addedExercises,
                AddedExercisesCount = addedExercisesCount,
                AddedWorkouts = addedWorkouts,
                AddedWorkoutsCount = addedWorkoutsCount
            };
            return View(model);
        }
        [Authorize]
        public async Task <IActionResult> Edit(string id)
        {
            if (User.Identity!.Name != id)
            {
                return Redirect("/");
            }

            User? user = await userService.GetUserAsync(id, true);

            EditProfileViewModel model = new EditProfileViewModel
            {
                Name = user.Name,
                Password = user.Password,
                Avatar = avatarService.GetAvatarUrl(user.Avatar),
                Description = user.Description,
            };
            return View(model);
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(string id, EditProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                User? user = await userService.GetUserAsync(id, false);
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
                user!.Name = model.Name;
                if (model.Password != null)
                {
                    if (model.Password == model.ConfirmPassword && model.Password.Length > 5)
                    {
                        user.Password = passwordCryptor.Hash(model.Password);
                    } else
                    {
                        TempData["Message"] = "Password must be at least 6 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character";
                    }
                }
                user.Description = model.Description ?? "No description";
                db.Users.Update(user);
                await db.SaveChangesAsync();

                return Redirect($"/Account/Profile/{id}");
            }
            logger.LogWarning("Model state is invalid.");
            return View(id); // Вернуться на ту же страницу в случае ошибки
        }
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete()
        {
            if (!userService.RemoveUserAsync(User.Identity!.Name).Result)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                HttpContext.Session.Clear();
                TempData["Message"] = "Something went wrong";
                return Redirect("/");
            }
            TempData["Message"] = "Account deleted successfully";
            return Redirect("/");
        }
    }
}

