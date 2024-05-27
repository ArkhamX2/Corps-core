namespace Corps.Server.Configuration.Repository
{
    public abstract class ConfigurationRepository(IConfiguration configuration)
    {
        protected IConfiguration Configuration { get; } = configuration;

        protected static string HandleStringValue(string? value, string exceptionMessage)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception(exceptionMessage);
            }

            return value;
        }

        protected static int HandleIntValue(int value, string exceptionMessage)
        {
            if (value <= 0)
            {
                throw new Exception(exceptionMessage);
            }

            return value;
        }
    }
}
