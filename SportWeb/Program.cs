using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SportWeb.Filters;
using SportWeb.Middlewares;
using SportWeb.Models;
using SportWeb.Models.Entities;
using SportWeb.Services;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<MessageFilter>();
    options.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseParameterTransformer()));
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
})
.AddJsonOptions(options =>
 {
     options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
 });
string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connectionString, o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
    });
var admins = builder.Configuration.GetSection("Admins").Get<List<string>>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var id = context.User.Identity?.Name;
            return id is not null && admins!.Contains(id);
        });
    });

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
    [
        "text/html",
        "text/css",
        "application/javascript",
        "text/javascript",
        "application/json",
        "application/xml",
        "text/xml",
        "image/svg+xml",
        "text/plain",
        "text/csv"
    ]);
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseQueryStrings = true;
    options.LowercaseUrls = true;
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);

builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();// добавляем IDistributedMemoryCache
builder.Services.AddSession();  // добавляем сервисы сессии

builder.Services.AddTransient<IAvatarService, AvatarService>();
builder.Services.AddTransient<IPasswordCryptorService, PasswordCryptorService>();
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IPictureService, PictureService>();
builder.Services.AddTransient<IPaginationService, PaginationService>();
builder.Services.AddScoped<IUserRepository, UserService>();
builder.Services.AddScoped<IUserRoleService, UserService>();
builder.Services.AddScoped<IUserCacheService, UserService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddScoped<IExerciseCacheService, ExerciseService>();
builder.Services.AddScoped<IWorkoutRepository, WorkoutService>();
builder.Services.AddScoped<IWorkoutEditorService, WorkoutService>();
builder.Services.AddScoped<IWorkoutCacheService, WorkoutService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddSingleton<IOutboundParameterTransformer, KebabCaseParameterTransformer>();
builder.Services.AddScoped<IFileUploadFacadeService, FileUploadFacadeService>();

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("NoUniqueContent", policy =>
    {
        policy.Expire(TimeSpan.FromHours(6));
    });

    options.AddPolicy("UserData", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(30));
        policy.SetVaryByRouteValue("id");
        policy.Tag("UserData");
    });

    options.AddPolicy("UserDataUnique", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(30));
        policy.SetVaryByRouteValue("id");
        policy.Tag("UserData");
        policy.Tag("UserDataUnique");
        policy.VaryByValue((context, token) =>
        {
            var id = context.User.Identity?.Name;
            return new ValueTask<KeyValuePair<string, string>>(
                new KeyValuePair<string, string>("User", id ?? ""));
        });
    });

    options.AddPolicy("NoCache", policy =>
    {
        policy.NoCache();
    });

    options.AddPolicy("ExerciseList", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(10));
        policy.Tag("ExerciseList");
    });

    options.AddPolicy("ExerciseListUnique", policy =>
    {
        policy.Expire(TimeSpan.FromMinutes(10));
        policy.Tag("ExerciseListUnique");
        policy.Tag("ExerciseList");
        policy.SetVaryByRouteValue("id");
        policy.VaryByValue((context, token) =>
        {
            var userCacheService = context.RequestServices.GetRequiredService<IUserCacheService>();
            var id = context.Request.RouteValues["id"]?.ToString();
            var isCurrentUser = userCacheService.IsCurrentUser(int.Parse(id ?? "0"));
            return new ValueTask<KeyValuePair<string, string>>(
                new KeyValuePair<string, string>("IsCurrentUser", isCurrentUser.ToString()));
        });
    });

    options.AddPolicy("WorkoutList", policy =>
    {

    });

    options.AddPolicy("IndexPage", policy =>
    {
        policy.Expire(TimeSpan.FromHours(6)); // Время жизни кэша
        policy.VaryByValue((context, token) =>
        {
            // Проверка, авторизован ли пользователь
            var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
            return new ValueTask<KeyValuePair<string, string>>(
                new KeyValuePair<string, string>("AuthStatus", isAuthenticated.ToString()));
        });
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.AddTokenBucketLimiter("LoginRateLimit", tokenOptions =>
    {
        tokenOptions.TokenLimit = 10; // Максимум 10 токенов
        tokenOptions.ReplenishmentPeriod = TimeSpan.FromMinutes(1); // Период пополнения токенов
        tokenOptions.TokensPerPeriod = 1; // Добавление 1 токена за период
        tokenOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        tokenOptions.QueueLimit = 2; // Очередь до 2 запросов
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Слишком много запросов. Пожалуйста, попробуйте позже.", cancellationToken);
    };
});

// Добавляем настройки reCAPTCHA
builder.Services.Configure<GoogleReCaptchaSettings>(builder.Configuration.GetSection("GoogleReCaptcha"));

// Добавляем HttpClient для использования в сервисе проверки reCAPTCHA
builder.Services.AddHttpClient();

// Добавляем сервис для проверки reCAPTCHA
builder.Services.AddTransient<IReCaptchaService, ReCaptchaService>();

builder.Services.AddOutputCache();

var app = builder.Build();

app.UseOutputCache();

app.UseMiddleware<RequestLoggingMiddleware>();
// Исключение сжатия для малых ответов
app.Use(async (context, next) =>
{
    await next();

    var contentLength = context.Response.ContentLength;
    if (contentLength.HasValue && contentLength.Value < 1024)
    {
        context.Response.Headers.Remove("Content-Encoding");
        app.Logger.LogInformation("Response wasn't compressed");
    }
    else
    {
        app.Logger.LogInformation("Response was compressed");
    }
});

//Защита от Clickjacking
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    await next();
});

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}
var options = new RewriteOptions().AddRedirectToHttpsPermanent();
app.UseRewriter(options);
app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions()
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.CacheControl = "public, max-age:600";
    }
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();