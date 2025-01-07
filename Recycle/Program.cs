
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Recycle.Api.BackgroundServices;
using Recycle.Api.Services;
using Recycle.Api.Settings;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities.Identity;

namespace Recycle.Api;

public class Program
{
    private static string ContentRootPath = Directory.GetCurrentDirectory();

    public static async Task Main(string[] args)
    {
        var ContentRootPath = Directory.GetCurrentDirectory();

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Host.UseContentRoot(ContentRootPath);

        builder.Services.AddSingleton<IClock>(SystemClock.Instance);
        builder.Services.AddScoped<IApplicationMapper, ApplicationMapper>();

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, builder =>
            {
                builder.UseNodaTime();
            });
        });

        builder.Services.AddControllers().AddNewtonsoftJson();

        builder.Services.AddIdentity<ApplicationUser, Role>(options =>
            options.SignIn.RequireConfirmedAccount = true
            )
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;
        });

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
        builder.Services.AddScoped<EmailSenderService>();
        builder.Services.AddHostedService<EmailSenderBackgroundService>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.usehttpsredirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}

