using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RbacApi.Infrastructure;
using RbacApi.Repositories;
using RbacApi.Repositories.Interfaces;
using RbacApi.Services;
using RbacApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── RSA 2048 金鑰對 (Singleton，啟動時動態生成) ──
var rsa = RSA.Create(2048);
builder.Services.AddSingleton(rsa);

// 僅含公鑰的 RSA 實例，供 JWT 驗證
var rsaPublicOnly = RSA.Create();
rsaPublicOnly.ImportParameters(rsa.ExportParameters(false));

// ── Infrastructure ──
builder.Services.AddSingleton<DbConnectionFactory>();

// ── Repositories ──
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ── Services ──
builder.Services.AddSingleton<IJwtService>(sp =>
    new JwtService(rsa, builder.Configuration));
builder.Services.AddScoped<IAuthService, AuthService>();

// ── JWT Bearer Authentication (RS256) ──
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["JWT:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new RsaSecurityKey(rsaPublicOnly),
        };
    });

builder.Services.AddAuthorization();

// ── CORS ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",  // demo-consumer
                "http://localhost:3001",  // demo-provider
                "http://localhost:3002",  // common-provider
                "http://localhost:5200")  // consumer-api
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ── Controllers ──
builder.Services.AddControllers();

var app = builder.Build();

// ── Middleware Pipeline ──
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
