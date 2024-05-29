
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Corps.Server.JWT;
using System;
using Corps.Server.Utils;
using Corps.Server.Hubs;
using Microsoft.Extensions.Options;
using Corps.Server.Configuration.Repository;
using Corps.Server.Data.Factory;
using Corps.Server.Data;
using Microsoft.Extensions.Configuration;
using Corps.Server.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Corps.Server.Services;

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
        services.AddTransient<ConfigurationManager>();
        services.AddControllers();
    }
    private static void RegisterDataSources(IServiceCollection services, DataConfigurationManager dataConfiguration)
    {
        var identityDataConfiguration = dataConfiguration.DataConfiguration.GetIdentityContextConfiguration(IsDebugMode);

        services.AddSingleton<IContextFactory<IdentityContext>>(new IdentityContextFactory(identityDataConfiguration));

        services.AddScoped(provider =>
        {
            var factory = provider.GetService<IContextFactory<IdentityContext>>();

            return factory!.CreateDataContext();
        });
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

                        // ���� ������ ��������� ����
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/game"))
                        {
                           // �������� ����� �� ������ �������
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

    private static void Main(string[] args)
    {       
        var builder = WebApplication.CreateBuilder(args);

        var hosts = new List<GameHost>
        {
        new GameHost("tom@gmail.com", "12345"),
        new GameHost("bob@gmail.com", "55555")
        };

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = AuthOptions.ISSUER,
                ValidateAudience = true,
                ValidAudience = AuthOptions.AUDIENCE,
                ValidateLifetime = true,
                IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                ValidateIssuerSigningKey = true
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];

                    // ���� ������ ��������� ����
                    var path = context.HttpContext.Request.Path;
                    if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/game"))
                    {
                        // �������� ����� �� ������ �������
                        context.Token = accessToken;
                    }
                    return Task.CompletedTask;
                }
            };
        });

        builder.Services.AddControllers();
        builder.Services.AddSignalR();
        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();
        app.MapPost("/login", (GameHost loginModel) =>
    {
        // ������� ������������ 
        GameHost? host = hosts.FirstOrDefault(p => p.Name == loginModel.Name && p.Password == loginModel.Password);
        // ���� ������������ �� ������, ���������� ��������� ��� 401
        if (host is null) return Results.Unauthorized();

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, host.Name) };
        // ������� JWT-�����
        var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        // ��������� �����
        var response = new
        {
            access_token = encodedJwt,
            username = host.Name
        };

        return Results.Json(response);
    });

        app.MapHub<GameHub>("/game");

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials
        }


        app.UseAuthentication();   // ���������� middleware �������������� 
        app.UseAuthorization();   // ���������� middleware ����������� 

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();

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

    //            // ���� ������ ��������� ����
    //            var path = context.HttpContext.Request.Path;
    //            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/game"))
    //            {
    //                // �������� ����� �� ������ �������
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
//    // ������� ������������ 
//    GameHost? host = hosts.FirstOrDefault(p => p.Name == loginModel.Name && p.Password == loginModel.Password);
//    // ���� ������������ �� ������, ���������� ��������� ��� 401
//    if (host is null) return Results.Unauthorized();

//    var claims = new List<Claim> { new Claim(ClaimTypes.Name, host.Name) };
//    // ������� JWT-�����
//    var jwt = new JwtSecurityToken(
//            issuer: AuthOptions.ISSUER,
//            audience: AuthOptions.AUDIENCE,
//            claims: claims,
//            expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(2)),
//            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
//    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

//    // ��������� �����
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


//app.UseAuthentication();   // ���������� middleware �������������� 
//app.UseAuthorization();   // ���������� middleware ����������� 

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapRazorPages();

//app.Run();

//}
