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
    public class AccountController : ControllerBase
    {
        readonly IAvatarService avatarService;
        readonly IPasswordCryptor passwordCryptor;
        public AccountController(ApplicationContext context, ILogger<AccountController> logger, IUserService userService, IPasswordCryptor passwordCryptor, IAvatarService avatarService, IFileService fileService)
            : base(context, logger, userService, fileService)
        {
            this.passwordCryptor = passwordCryptor;
            this.avatarService = avatarService;
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Login(User form)
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

        public IActionResult Profile(string id)
        {
            User? user = userService.GetUserAsync(id).Result;
            if (user == null)
            {
                return NotFound();
            }
            var isUserProfile = User.Identity!.Name == id;//User != null && User.HasClaim(c => c.Type == "Id") && User.FindFirst(c => c.Type == "Id").Value == Model.Id.ToString();
            var model = new ProfileViewModel
            {
                IsUserProfile = isUserProfile,
                Name = user.Name,
                Avatar = avatarService.GetAvatarUrl(user.Avatar),
                Description = user.Description,
                Id = user.Id.ToString()
            };
            return View(model);
        }
        [Authorize]
        public IActionResult Edit(string id, string returnUrl)
        {
            if (User.Identity!.Name != id)
            {
                return Redirect("/");
            }

            User? user = userService.GetUserAsync(id).Result;

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
                User? user = await userService.GetUserAsync(id);
                if (user == null)
                {
                    logger.LogWarning($"User with id {id} not found.");
                    return NotFound();
                }

                var fileUpload = model.FileUpload;
                if (fileUpload != null && fileUpload.Length > 0)
                {
                    var filePath = avatarService.GetAvatarPath(avatarService.NewAvatarName(user));
                    await fileService.UploadFile(fileUpload, filePath);
                    user.Avatar = filePath;
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

