using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services, builder.Configuration);
var app = builder.Build();

ConfigureMiddleware(app);
ConfigureEndpoints(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddControllersWithViews();
    services.AddDistributedMemoryCache();

    ConfigureAuthentication(services, configuration);
    ConfigureSession(services);
    ConfigureDatabase(services, configuration);
}

void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = "Google";
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    })
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
        options.CallbackPath = "/signin-google";
    });
}

void ConfigureSession(IServiceCollection services)
{
    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });
}

void ConfigureDatabase(IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
}

void ConfigureMiddleware(WebApplication app)
{
    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseSession();
}

void ConfigureEndpoints(WebApplication app)
{
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Auth}/{action=Login}/{id?}");
}
