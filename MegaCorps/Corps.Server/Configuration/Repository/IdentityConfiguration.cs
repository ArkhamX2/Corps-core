using Microsoft.AspNetCore.Identity;

namespace Corps.Server.Configuration.Repository
{
    public class IdentityConfiguration(IConfiguration configuration):ConfigurationRepository(configuration)
    {
        private bool GetPasswordRequireDigit()
        {
            return Configuration
                .GetSection("Identity")
                .GetSection("Password")
                .GetValue<bool>("RequireDigit");
        }

        private bool GetPasswordRequireLowercase()
        {
            return Configuration
                .GetSection("Identity")
                .GetSection("Password")
                .GetValue<bool>("RequireLowercase");
        }

        private bool GetPasswordRequireUppercase()
        {
            return Configuration
                .GetSection("Identity")
                .GetSection("Password")
                .GetValue<bool>("RequireUppercase");
        }

        private bool GetPasswordRequireNonAlphanumeric()
        {
            return Configuration
                .GetSection("Identity")
                .GetSection("Password")
                .GetValue<bool>("RequireNonAlphanumeric");
        }

        private int GetPasswordRequiredLength()
        {
            var result = Configuration
                .GetSection("Identity")
                .GetSection("Password")
                .GetValue<int>("RequiredLength");

            return HandleIntValue(result, "Password required length is invalid!");
        }

        private int GetPasswordRequiredUniqueChars()
        {
            var result = Configuration
                .GetSection("Identity")
                .GetSection("Password")
                .GetValue<int>("RequiredUniqueChars");

            return HandleIntValue(result, "Password required unique chars is invalid!");
        }

        private bool GetUserRequireUniqueEmail()
        {
            return Configuration
                .GetSection("Identity")
                .GetSection("User")
                .GetValue<bool>("RequireUniqueEmail");
        }

        public IdentityOptions GetOptions()
        {
            return new IdentityOptions
            {
                Password = new PasswordOptions
                {
                    RequireDigit = GetPasswordRequireDigit(),
                    RequireLowercase = GetPasswordRequireLowercase(),
                    RequireUppercase = GetPasswordRequireUppercase(),
                    RequireNonAlphanumeric = GetPasswordRequireNonAlphanumeric(),
                    RequiredLength = GetPasswordRequiredLength(),
                    RequiredUniqueChars = GetPasswordRequiredUniqueChars()
                },
                User = new UserOptions
                {
                    RequireUniqueEmail = GetUserRequireUniqueEmail()
                }
            };
        }
    }
}
