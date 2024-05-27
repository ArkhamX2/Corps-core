using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Corps.Server.Configuration.Repository
{
    public class TokenConfiguration(IConfiguration configuration): ConfigurationRepository(configuration)
    {
        public string GetIssuer()
        {
            var result = Configuration
                .GetSection("Token")
                .GetValue<string>("Issuer");

            return HandleStringValue(result, "Token issuer is null or empty!");
        }

        public string GetAudience()
        {
            var result = Configuration
                .GetSection("Token")
                .GetValue<string>("Audience");

            return HandleStringValue(result, "Token audience is null or empty!");
        }

        public string GetClaimNameSub()
        {
            var result = Configuration
                .GetSection("Token")
                .GetValue<string>("ClaimNameSub");

            return HandleStringValue(result, "Token claim name sub is null or empty!");
        }

        private string GetKey()
        {
            var result = Configuration
                .GetSection("Token")
                .GetValue<string>("Key");

            return HandleStringValue(result, "Token key is null or empty!");
        }

        private int GetClockSkew()
        {
            var result = Configuration
                .GetSection("Token")
                .GetValue<int>("ClockSkewMinutes");

            return HandleIntValue(result, "Token clock skew is invalid!");
        }

        public int GetLifetimeMinutes()
        {
            var result = Configuration
                .GetSection("Token")
                .GetValue<int>("LifetimeMinutes");

            return HandleIntValue(result, "Token lifetime is invalid!");
        }

        public SymmetricSecurityKey GetSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetKey()));
        }

        public TokenValidationParameters GetValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = GetIssuer(),
                ValidAudience = GetAudience(),
                ClockSkew = TimeSpan.FromMinutes(GetClockSkew()),
                IssuerSigningKey = GetSecurityKey()
            };
        }
    }
}
