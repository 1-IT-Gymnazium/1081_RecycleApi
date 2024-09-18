using NodaTime;

namespace Recycle.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;


        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IClock>(SystemClock.Instance);
        }
    }
}
