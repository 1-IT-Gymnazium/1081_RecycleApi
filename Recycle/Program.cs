
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NodaTime;
using Recycle.Data;

namespace Recycle.Api;

public class Program
{
    private static string ContentRootPath = Directory.GetCurrentDirectory();

    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.AddSingleton<IClock>(SystemClock.Instance);

        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString, builder =>
            {
                builder.UseNodaTime();
            });
        });

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

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();

    }
}

