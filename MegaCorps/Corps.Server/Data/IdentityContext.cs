using Corps.Server.Data.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Corps.Server.Data
{
    public class IdentityContext(ContextConfiguration configuration) : IdentityDbContext<IdentityUser>
    {
        private ContextConfiguration Configuration { get; } = configuration;

        public bool TryInitialize()
        {
            var canConnect = Database.CanConnect();
            var isCreated = Database.EnsureCreated();

            return canConnect || isCreated;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            Configuration.ConfigureContext(optionsBuilder);

            base.OnConfiguring(optionsBuilder);
        }
    }
}
