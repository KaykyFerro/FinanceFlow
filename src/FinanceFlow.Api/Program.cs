using System.Net;
using System.Text;
using FinanceFlow.Api.Authentication;
using FinanceFlow.Domain.Entities;
using FinanceFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Railway provides PORT at runtime. Bind ASP.NET Core to 0.0.0.0:$PORT.
var railwayPort = Environment.GetEnvironmentVariable("PORT");
if (int.TryParse(railwayPort, out var port) && port > 0)
{
    builder.WebHost.ConfigureKestrel(options => options.Listen(IPAddress.Any, port));
}

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) || Encoding.UTF8.GetByteCount(jwtOptions.SecretKey) < 32)
    throw new InvalidOperationException("Jwt:SecretKey deve ter pelo menos 32 bytes. Configure via variável de ambiente ou User Secrets.");

// Railway exposes the database through DATABASE_URL. Prefer it over the local appsettings value.
// DATABASE_URL is normally a postgres:// URI, while Npgsql expects a keyword=value connection string.
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
var connectionString = !string.IsNullOrWhiteSpace(databaseUrl)
    ? ToNpgsqlConnectionString(databaseUrl)
    : builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("DATABASE_URL ou ConnectionStrings:DefaultConnection não está configurada.");

builder.Services.AddDbContext<FinanceFlowDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthTokenService>();
builder.Services.AddSingleton<IEmailSender, DevelopmentEmailSender>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

builder.Services.AddCors(options => options.AddPolicy("Web", policy =>
    policy.WithOrigins(
            "https://kaykyferro.github.io",
            "http://localhost:5173",
            "http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

// Lightweight public health endpoint that does not depend on controller discovery or the database.
app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    service = "FinanceFlow.Api"
}));

// Keep startup resilient while the database is coming online. Once the connection works,
// EnsureCreatedAsync creates the MVP schema automatically. A proper EF migration pipeline
// will replace this later.
try
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<FinanceFlowDbContext>();
    await db.Database.EnsureCreatedAsync();
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "Database initialization failed. The API will remain online; database-backed endpoints may be unavailable.");
}

// Railway terminates HTTPS at its edge. Locally, keep the normal ASP.NET HTTPS setup.
if (string.IsNullOrWhiteSpace(railwayPort))
{
    app.UseHttpsRedirection();
}

app.UseCors("Web");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static string ToNpgsqlConnectionString(string value)
{
    if (!value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
        !value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        return value;
    }

    var uri = new Uri(value);
    var userInfo = uri.UserInfo.Split(':', 2);
    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.IsDefaultPort ? 5432 : uri.Port,
        Database = uri.AbsolutePath.TrimStart('/'),
        Username = Uri.UnescapeDataString(userInfo[0]),
        Password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty,
        SslMode = SslMode.Require
    };

    return builder.ConnectionString;
}
