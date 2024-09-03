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
using SportWeb.Services;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
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
            return id != null && admins!.Contains(id);
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

builder.Services.AddDistributedMemoryCache();// ��������� IDistributedMemoryCache
builder.Services.AddSession();  // ��������� ������� ������

builder.Services.AddTransient<IAvatarService, AvatarService>();
builder.Services.AddTransient<IPasswordCryptor, PasswordCryptor>();
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IPictureService, PictureService>();
builder.Services.AddTransient<IPaginationService, PaginationService>();
builder.Services.AddScoped<IUserRepository, UserService>();
builder.Services.AddScoped<IUserRoleService, UserService>();
builder.Services.AddScoped<IUserSessionService, UserService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddScoped<IExerciseService, ExerciseService>();
builder.Services.AddSingleton<IOutboundParameterTransformer, KebabCaseParameterTransformer>();

builder.Services.AddRateLimiter(options =>
{
    options.AddTokenBucketLimiter("LoginRateLimit", tokenOptions =>
    {
        tokenOptions.TokenLimit = 10; // �������� 10 �������
        tokenOptions.ReplenishmentPeriod = TimeSpan.FromMinutes(1); // ������ ���������� �������
        tokenOptions.TokensPerPeriod = 1; // ���������� 1 ������ �� ������
        tokenOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        tokenOptions.QueueLimit = 2; // ������� �� 2 ��������
    });
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("������� ����� ��������. ����������, ���������� �����.", cancellationToken);
    };
});

// ��������� ��������� reCAPTCHA
builder.Services.Configure<GoogleReCaptchaSettings>(builder.Configuration.GetSection("GoogleReCaptcha"));

// ��������� HttpClient ��� ������������� � ������� �������� reCAPTCHA
builder.Services.AddHttpClient();

// ��������� ������ ��� �������� reCAPTCHA
builder.Services.AddTransient<IReCaptchaService, ReCaptchaService>();

builder.Services.AddOutputCache();

var app = builder.Build();

app.UseOutputCache();

app.UseMiddleware<RequestLoggingMiddleware>();
// ���������� ������ ��� ����� �������
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

//������ �� Clickjacking
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