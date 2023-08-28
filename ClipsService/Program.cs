using ClipsService.Auth;
using ClipsService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace ClipsService;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        // Swagger Setup
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Authentication Setup
        var domain = $"https://{Environment.GetEnvironmentVariable("Auth0Domain")}/";
        var audiance = Environment.GetEnvironmentVariable("Auth0Audience");
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = domain;
                options.Audience = audiance;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });

        // Authorization Setup
        builder.Services.AddAuthorization( options =>
        {

            options.AddPolicy("AbleToWriteMyClips", policy => policy.Requirements.Add(new
                HasAnyPermRequirement( new List<string>() { "myclips:write" }, domain)));

            options.AddPolicy("AbleToReadMyClips", policy => policy.Requirements.Add(new
                HasAnyPermRequirement( new List<string>() { "myclips:read" }, domain)));

            options.AddPolicy("AbleToWriteOtherUserClips", policy => policy.Requirements.Add(new
                HasAnyPermRequirement( new List<string>() { "userclips:write" }, domain)));

            options.AddPolicy("AbleToWriteUsers", policy => policy.Requirements.Add(new
                HasAnyPermRequirement( new List<string>() { "users:write" }, domain)));
        });
        builder.Services.AddSingleton<IAuthorizationHandler, HasAnyPermRequirementHandler>();

        // Database Setup
        var connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");
        builder.Services.AddSingleton<CosmosClient>(s => new CosmosClient(connectionString)); ;
        builder.Services.AddTransient<Services.IClipsService, Services.ClipService>();
        builder.Services.AddTransient<Services.IUserService, Services.UserService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
