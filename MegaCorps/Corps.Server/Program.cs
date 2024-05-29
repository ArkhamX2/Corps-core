
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Corps.Server.Data;
using Corps.Server.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
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





   
