using Corps.Server.Data.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Corps.Server.Data.Factory
{
    public class IdentityContextFactory : IContextFactory<IdentityContext>
    {
        private readonly ContextConfiguration _configuration;

        public IdentityContextFactory(ContextConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DbContext CreateDataContext()
        {
            var context = new IdentityContext(_configuration);
            if (!context.TryInitialize())
            {
                throw new Exception("Failed to initialize identity context!");
            }
            return context;
        }
    }
}
