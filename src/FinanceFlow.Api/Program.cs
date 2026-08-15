using System.Net;
using System.Text;
using FinanceFlow.Api.Authentication;
using FinanceFlow.Domain.Entities;
using FinanceFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:DefaultConnection não está configurada.");

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

// Create the initial schema automatically for the first hosted deployment.
// This is intentionally temporary for the MVP; proper EF migrations will replace it later.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanceFlowDbContext>();
    await db.Database.EnsureCreatedAsync();
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
