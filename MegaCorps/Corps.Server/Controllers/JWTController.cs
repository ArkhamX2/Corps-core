
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using Corps.Server.Data;
using Corps.Server.Services;
using Microsoft.AspNetCore.Identity;
using Corps.Server.DTO;
using Corps.Server.Utils.JSON;
using Microsoft.AspNetCore.Identity.Data;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;


namespace Corps.Server.Controllers
{
    [Route("jwt/")]
    [ApiController]
    public class JWTController(
        IdentityContext identityContext,
        TokenService tokenService,
        UserManager<IdentityUser> userManager
        ) : ControllerBase
    {
        private IdentityContext identityContext = identityContext;
        private  TokenService tokenService = tokenService;
        private  UserManager<IdentityUser> userManager = userManager;

        [HttpPost("login")]
        public async Task<ActionResult<SecurityResponse>> Login([FromBody] SecurityRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(request);
            }

            var identityUser = await userManager.FindByEmailAsync(request.login);

            if (identityUser is null)
            {
                return Unauthorized();
            }

            if (!await userManager.CheckPasswordAsync(identityUser, request.password))
            {
                return Unauthorized();
            }

                       
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

            var identityUser = await identityContext.Users.FirstOrDefaultAsync(user => user.Email==request.login);


            if (identityUser is null)
            {
                throw new Exception("Internal error! Please try again.");
            }
                     
            return Ok(DataSerializer.Serialize(new SecurityResponse
            {
                host = request.login,
                Token = tokenService.CreateNewToken(identityUser)
            }));
        }


    }
}

