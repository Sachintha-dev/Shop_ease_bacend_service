using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ECommerceBackend.Models;
using ECommerceBackend.Data.Contexts;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug); // Set to Debug for detailed logs
builder.Services.AddControllers();

// Add services to the container.

// Add controllers
builder.Services.AddControllers();

// Configure DatabaseSettings
builder.Services.Configure<DatabaseSettings>(builder.Configuration.GetSection(nameof(DatabaseSettings)));

// **Check DatabaseSettings here**
var databaseSettingsSection = builder.Configuration.GetSection(nameof(DatabaseSettings));
var databaseSettings = databaseSettingsSection.Get<DatabaseSettings>();

if (databaseSettings == null)
{
    throw new Exception("Database settings are not configured properly in appsettings.json.");
}

if (string.IsNullOrEmpty(databaseSettings.ConnectionString) ||
    string.IsNullOrEmpty(databaseSettings.DatabaseName))
{
    throw new Exception("Database settings are missing required properties.");
}

// Register MongoDbContext
builder.Services.AddSingleton<MongoDbContext>();

// Configure JwtSettings
var jwtSettingsSection = builder.Configuration.GetSection(nameof(JwtSettings));
builder.Services.Configure<JwtSettings>(jwtSettingsSection);

// **Check JwtSettings here**
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

if (jwtSettings == null)
{
    throw new Exception("JWT settings are not configured properly in appsettings.json.");
}

if (string.IsNullOrEmpty(jwtSettings.Secret) ||
    string.IsNullOrEmpty(jwtSettings.Issuer) ||
    string.IsNullOrEmpty(jwtSettings.Audience))
{
    throw new Exception("JWT settings are missing required properties.");
}

var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        ClockSkew = TimeSpan.Zero
    };
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

// Configure Health Checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// Map health check endpoint
app.MapHealthChecks("/health");

app.Run();
