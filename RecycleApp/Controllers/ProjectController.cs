using Microsoft.AspNetCore.Mvc;
using NodaTime;
using RecycleApp.Data;

namespace RecycleApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly ILogger<ProjectController> _logger;
        private readonly IClock _clock;
        private readonly AppDbContext _dbContext;

        public ProjectController(
            ILogger<ProjectController> logger,
            IClock clock,
            AppDbContext dbContext)
        {
            _logger = logger;
            _clock = clock;
            _dbContext = dbContext;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
            })
            .ToArray();
        }
    }
}
