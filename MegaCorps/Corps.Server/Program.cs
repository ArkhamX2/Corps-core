
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Corps.Server.Data;
using Corps.Server.Configuration;
using Microsoft.AspNetCore.Identity;
using Corps.Server.Services;
using Corps.Server.Configuration.Repository;
using Corps.Server.Data.Initialization;
using Corps.Server.Hubs;

internal class Program
{
#if DEBUG
    private const bool IsDebugMode = true;
#else
    private const bool IsDebugMode = false;
#endif

    private static void RegisterCoreServices(IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<TokenService>();
        services.AddTransient<DataConfigurationManager>();
        services.AddControllers();
    }
    private static void RegisterDataSources(IServiceCollection services, DataConfigurationManager dataConfiguration)
    {
        var identityDataConfiguration = dataConfiguration.DataConfiguration.GetIdentityContextConfiguration(IsDebugMode);

        services.AddScoped(provider => new IdentityContext(identityDataConfiguration));

        services.AddScoped<IdentityInitializationScript>();
    }
    private static void RegisterIdentityServices(IServiceCollection services, DataConfigurationManager configuration)
    {
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<IdentityContext>()
            .AddUserManager<UserManager<IdentityUser>>()
            .AddRoleManager<RoleManager<IdentityRole>>()
            .AddSignInManager<SignInManager<IdentityUser>>();

        services.Configure<IdentityOptions>(options => configuration.IdentityConfiguration.GetOptions());
    }
    private static void RegisterSecurityServices(IServiceCollection services, DataConfigurationManager configuration)
    {
        services.AddAuthorization();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.IncludeErrorDetails = true;
            options.TokenValidationParameters = configuration.TokenConfiguration.GetValidationParameters();
            options.Events = new JwtBearerEvents
            {
                    OnMessageReceived = context =>
                    {
                       var accessToken = context.Request.Query["access_token"];

                        // если запрос направлен хабу
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/game"))
                        {
                           // получаем токен из строки запроса
                            context.Token = accessToken;
                        }
                       return Task.CompletedTask;
                    }
            };

        });

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });
    }
    private static async void InitializeDataSources(WebApplication application)
    {
        using var scope = application.Services.CreateScope();

        await scope.ServiceProvider.GetRequiredService<IdentityInitializationScript>().Run();
    }

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;
        var configuration = new DataConfigurationManager(builder.Configuration);
        

        RegisterCoreServices(services);
        RegisterDataSources(services, configuration);
        RegisterIdentityServices(services, configuration);
        RegisterSecurityServices(services, configuration);

        var application = builder.Build();
        application.MapHub<GameHub>("/game");
        application.UseAuthentication();
        application.UseAuthorization();
        application.MapControllers();
        application.UseCors();

        InitializeDataSources(application);

        application.Run();

    }


}




    //var builder = WebApplication.CreateBuilder(args);

    //// Add services to the container.
    //builder.Services.AddRazorPages();
    //builder.Services.AddAuthorization();
    //builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    //.AddJwtBearer(options =>
    //{
    //    options.TokenValidationParameters = new TokenValidationParameters
    //    {
    //        ValidateIssuer = true,
    //        ValidIssuer = AuthOptions.ISSUER,
    //        ValidateAudience = true,
    //        ValidAudience = AuthOptions.AUDIENCE,
    //        ValidateLifetime = true,
    //        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
    //        ValidateIssuerSigningKey = true
    //    };

    //    options.Events = new JwtBearerEvents
    //    {
    //        OnMessageReceived = context =>
    //        {
    //            var accessToken = context.Request.Query["access_token"];

    //            // если запрос направлен хабу
    //            var path = context.HttpContext.Request.Path;
    //            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/game"))
    //            {
    //                // получаем токен из строки запроса
    //                context.Token = accessToken;
    //            }
    //            return Task.CompletedTask;
    //        }
    //    };
    //});

//builder.Services.AddControllers();
//builder.Services.AddSignalR();
//builder.Services.AddEndpointsApiExplorer();

//var app = builder.Build();
//    app.MapPost("/login", (GameHost loginModel) =>
//{
//    // находим пользователя 
//    GameHost? host = hosts.FirstOrDefault(p => p.Name == loginModel.Name && p.Password == loginModel.Password);
//    // если пользователь не найден, отправляем статусный код 401
//    if (host is null) return Results.Unauthorized();

//    var claims = new List<Claim> { new Claim(ClaimTypes.Name, host.Name) };
//    // создаем JWT-токен
//    var jwt = new JwtSecurityToken(
//            issuer: AuthOptions.ISSUER,
//            audience: AuthOptions.AUDIENCE,
//            claims: claims,
//            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
//            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
//    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

//    // формируем ответ
//    var response = new
//    {
//        access_token = encodedJwt,
//        username = host.Name
//    };

//    return Results.Json(response);
//});

//app.MapHub<GameHub>("/game");

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}
//else
//{
//    app.UseCors(x => x
//        .AllowAnyMethod()
//        .AllowAnyHeader()
//        .SetIsOriginAllowed(origin => true) // allow any origin
//        .AllowCredentials()); // allow credentials
//}


//app.UseAuthentication();   // добавление middleware аутентификации 
//app.UseAuthorization();   // добавление middleware авторизации 

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapRazorPages();

//app.Run();

//}
