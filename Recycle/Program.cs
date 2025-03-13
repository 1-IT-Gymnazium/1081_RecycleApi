
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NodaTime;
using Recycle.Api.BackgroundServices;
using Recycle.Api.Services;
using Recycle.Api.Settings;
using Recycle.Api.Utilities;
using Recycle.Data;
using Recycle.Data.Entities.Identity;
using System.Text;

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
        builder.Services.AddScoped<IImageService, ImageService>();

        var baseUploadsFolder = @"C:\Elareinstaluje\repos\RecycleApi\Recycle\Uploads";
        var profilePicturesFolder = Path.Combine(baseUploadsFolder, "ProfilePictures");
        var productImagesFolder = Path.Combine(baseUploadsFolder, "ProductImages");
        var trashCanImagesFolder = Path.Combine(baseUploadsFolder, "TrashCanImages");
        var articleImagesFolder = Path.Combine(baseUploadsFolder, "ArticleImages");

        foreach (var folder in new[] { baseUploadsFolder, profilePicturesFolder, productImagesFolder, trashCanImagesFolder, articleImagesFolder })
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Console.WriteLine($"Uploads folder created at: {folder}");
            }
            else
            {
                Console.WriteLine($"Uploads folder already exists at: {folder}");
            }
        }

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, builder =>
            {
                builder.UseNodaTime();
            });
        });

        builder.Services.AddControllers().AddNewtonsoftJson();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            // Disable email confirmation requirement for sign-in
            options.SignIn.RequireConfirmedAccount = false;
        })
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

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));

        var jwtSettings = builder.Configuration.GetRequiredSection(nameof(JwtSettings)).Get<JwtSettings>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience
            };
        });

        builder.Services.Configure<EnviromentSettings>(builder.Configuration.GetSection("EnvironmentSettings"));
        builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
        builder.Services.AddScoped<EmailSenderService>();
        builder.Services.AddHostedService<EmailSenderBackgroundService>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "JWT API", Version = "v1" });

            // Configure JWT Authentication in Swagger
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token without the 'Bearer' prefix.\n\nExample: abc123xyz"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
        var uploadsFolder = @"C:\Elareinstaluje\repos\RecycleApi\Recycle\Uploads";
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
            Console.WriteLine($"Uploads folder created at: {uploadsFolder}");
        }
        else
        {
            Console.WriteLine($"Uploads folder already exists at: {uploadsFolder}");
        }

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(baseUploadsFolder),
            RequestPath = "/Uploads"
        });

        //app.usehttpsredirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}

