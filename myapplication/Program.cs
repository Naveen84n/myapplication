using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using myapplication.Configuration;
using myapplication.Data;
using myapplication.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplicationInsightsTelemetry();
var azureAdOptions = builder.Configuration.GetSection("AzureAd").Get<AzureAdOptions>()
    ?? throw new InvalidOperationException("AzureAd configuration is missing.");

if (string.IsNullOrWhiteSpace(azureAdOptions.TenantId) ||
    string.IsNullOrWhiteSpace(azureAdOptions.ClientId))
{
    throw new InvalidOperationException(
        "AzureAd:TenantId and AzureAd:ClientId must be configured before starting the API.");
}

// Register Azure token service for development token endpoint
builder.Services.AddSingleton(azureAdOptions);
builder.Services.AddScoped<IAzureTokenService, AzureTokenService>();

var validAudiences = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    azureAdOptions.ClientId,
    $"api://{azureAdOptions.ClientId}"
};

if (!string.IsNullOrWhiteSpace(azureAdOptions.Audience))
{
    validAudiences.Add(azureAdOptions.Audience);
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = azureAdOptions.Authority;
        options.RequireHttpsMetadata = true;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = azureAdOptions.Authority,
            ValidateAudience = true,
            ValidAudiences = validAudiences,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddControllers();
var keyVaultUri = builder.Configuration["KeyVault:VaultUri"]
    ?? throw new InvalidOperationException("KeyVault:VaultUri is not configured.");
builder.Services.AddSingleton(_ => new SecretClient(
    new Uri(keyVaultUri),
    new DefaultAzureCredential()));
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyAppConnectionstring")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter an Entra ID access token. Do not include the 'Bearer' prefix. Get a token from the /token/get-token endpoint."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document, null),
            new List<string>()
        }
    });

    options.OrderActionsBy(api =>
    {
        var tagAttr = api.ActionDescriptor.EndpointMetadata
            .OfType<TagsAttribute>()
            .FirstOrDefault();

        if (tagAttr?.Tags.Contains("Token") == true)
        {
            return "0_Token";
        }

        var controllerName = api.ActionDescriptor.RouteValues.ContainsKey("controller")
            ? api.ActionDescriptor.RouteValues["controller"]
            : "Unknown";

        var actionName = api.ActionDescriptor.RouteValues.ContainsKey("action")
            ? api.ActionDescriptor.RouteValues["action"]
            : "Unknown";

        return $"1_{controllerName}_{actionName}";
    });
});


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

