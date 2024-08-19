using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using SportWeb.Models;
using SportWeb.Services;
using SportWeb.Filters;
using Microsoft.AspNetCore.Rewrite;
using System.Text.Json.Serialization;
using SportWeb.Middlewares;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.  
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<MessageFilter>();
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
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAssertion(context =>
        {
            var id = context.User.Identity?.Name;
            return id != null && admins!.Contains(id);
        });
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
builder.Services.Configure<BrotliCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);

builder.Services.AddHttpContextAccessor();

builder.Services.AddDistributedMemoryCache();// добавляем IDistributedMemoryCache
builder.Services.AddSession();  // добавляем сервисы сессии

builder.Services.AddTransient<IAvatarService, AvatarService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IPasswordCryptor, PasswordCryptor>();
builder.Services.AddTransient<IFileService, FileService>();
builder.Services.AddTransient<IPictureService, PictureService>();
builder.Services.AddTransient<IPaginationService, PaginationService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();

var app = builder.Build();

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
    } else
    {
        app.Logger.LogInformation("Response was compressed");
    }
});

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
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
        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age:600";
    }
});

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
