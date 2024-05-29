using Corps.Server.Configuration;
using Corps.Server.Configuration.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Corps.Server.Services
{
    public class TokenService(DataConfigurationManager configuration)
    {
        private const string ClaimLabelUserId = ClaimTypes.NameIdentifier;
        private const string ClaimLabelUserName = ClaimTypes.Name;
        private const string ClaimLabelUserEmail = ClaimTypes.Email;
        private const string ClaimLabelUserRole = ClaimTypes.Role;

        private TokenConfiguration Configuration { get; } = configuration.TokenConfiguration;

        /// <summary>
        ///     Создать новый токен.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="roles">Роли.</param>
        /// <returns>Новый токен.</returns>
        public string CreateNewToken(IdentityUser user)
        {
            var claims = CreateTokenClaims(user);
            var lifetime = Configuration.GetLifetimeMinutes();
            var key = Configuration.GetSecurityKey();

            var token = new JwtSecurityToken(
                Configuration.GetIssuer(),
                Configuration.GetAudience(),
                claims,
                expires: DateTime.UtcNow.AddMinutes(lifetime),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        ///     Создать набор клеймов для токена.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="roles">Роли.</param>
        /// <returns>Набор клеймов.</returns>
        private IEnumerable<Claim> CreateTokenClaims(IdentityUser user)
        {
            var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Configuration.GetClaimNameSub()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new(ClaimLabelUserId, user.Id)
        };

            if (!string.IsNullOrEmpty(user.UserName))
            {
                claims.Add(new Claim(ClaimLabelUserName, user.UserName));
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                claims.Add(new Claim(ClaimLabelUserEmail, user.Email));
            }
           
            return claims;
        }
    }
}
