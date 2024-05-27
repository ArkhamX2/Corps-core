using Corps.Server.Configuration.Repository;
using Microsoft.Extensions.Configuration;

namespace Corps.Server.Configuration
{
    public class DataConfigurationManager(IConfiguration configuration)
    {
        public DataConfiguration DataConfiguration { get; } = new DataConfiguration(configuration);
        public IdentityConfiguration IdentityConfiguration { get; } = new IdentityConfiguration(configuration);
        public TokenConfiguration TokenConfiguration { get; } = new TokenConfiguration(configuration);
    }
}
