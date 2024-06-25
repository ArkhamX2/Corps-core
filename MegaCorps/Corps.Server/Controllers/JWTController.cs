using Corps.Server.Data;
using Corps.Server.DTO;
using Corps.Server.Services;
using Corps.Server.Utils.JSON;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Corps.Server.Controllers
{
    [Route("jwt/")]
    [ApiController]
    public class JWTController(
        IdentityContext identityContext,
        TokenService tokenService,
        UserManager<IdentityUser> userManager,
        ILogger<JWTController> logger
        ) : ControllerBase
    {
        private IdentityContext identityContext = identityContext;
        private TokenService tokenService = tokenService;
        private UserManager<IdentityUser> userManager = userManager;
        private ILogger<JWTController> logger = logger;

        [HttpPost("login")]
        public async Task<ActionResult<SecurityResponse>> Login([FromBody] SecurityRequest request)
        {
            if (!ModelState.IsValid)
            {
                logger.LogError("model isnt valid");
                return BadRequest(request);
            }

            var identityUser = await userManager.FindByEmailAsync(request.login);

            if (identityUser is null)
            {
                logger.LogError("User didnt exist");
                return Unauthorized();
            }

            if (!await userManager.CheckPasswordAsync(identityUser, request.password))
            {
                logger.LogError("Wrong Password");
                return Unauthorized();
            }

            logger.LogInformation("login success");
            return Ok(DataSerializer.Serialize(new SecurityResponse
            {
                host = request.login,
                Token = tokenService.CreateNewToken(identityUser)
            }));
        }

        /// <summary>
        ///     Регистрация пользователя.
        /// </summary>
        /// <param name="request">Тело запроса регистрации.</param>
        /// <returns>Тело ответа модуля безопасности.</returns>
        /// <exception cref="Exception">При неудачной попытке создания пользователя.</exception>
        [HttpPost("register")]
        public async Task<ActionResult<SecurityResponse>> Register([FromBody] SecurityRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(request);
            }

            var internalUser = new IdentityUser
            {
                UserName = request.login,
                Email = request.login
            };

            var result = await userManager.CreateAsync(internalUser, request.password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }

                return BadRequest(ModelState);
            }

            var identityUser = await identityContext.Users.FirstOrDefaultAsync(user => user.Email == request.login);


            if (identityUser is null)
            {
                throw new Exception("Internal error! Please try again.");
            }
            logger.LogInformation("register success");
            return Ok(DataSerializer.Serialize(new SecurityResponse
            {
                host = request.login,
                Token = tokenService.CreateNewToken(identityUser)
            }));
        }
        [Authorize]
        [HttpPost("token")]
        public async Task<IActionResult> RefreshToken()
        {
            if (User.Identity is null || !User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var user = await identityContext.Users
                .FirstOrDefaultAsync(user => user.UserName == User.Identity.Name);

            if (user is null || String.IsNullOrWhiteSpace(user.Email))
            {
                return Unauthorized();
            }

            return Ok(DataSerializer.Serialize(new SecurityResponse
            {
                host = user.Email,
                Token = tokenService.CreateNewToken(user)
            }));
        }


    }
}

