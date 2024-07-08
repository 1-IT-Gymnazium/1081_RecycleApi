using Microsoft.EntityFrameworkCore;

namespace RecycleApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
