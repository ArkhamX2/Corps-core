using Corps.Server.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Corps.Server.Configuration.Repository
{
    public class DataConfiguration(IConfiguration configuration) : ConfigurationRepository(configuration)
    {
        public ContextConfiguration GetIdentityContextConfiguration(bool isDebugMode)
        {
            var connectionString = Configuration.GetConnectionString("Identity");

            HandleStringValue(connectionString, "Identity connection string is null or empty!");

            return new SQliteConfiguration(connectionString!, isDebugMode);

        }
    }
}
