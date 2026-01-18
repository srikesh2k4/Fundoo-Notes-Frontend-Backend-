using BusinessLayer.Interfaces.Services;
using BusinessLayer.Services;
using DataBaseLayer.Context;
using DataBaseLayer.Interfaces;
using DataBaseLayer.Repositories;
using FundooNotes.Helpers;
using FundooNotes.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ModelLayer.Configuration;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========================================
// 1. CONFIGURATION SETTINGS
// ========================================
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// ========================================
// 2. DATABASE CONFIGURATION
// ========================================
var connectionString = builder.Configuration.GetConnectionString("FundooConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Database connection string 'FundooConnection' is not configured in appsettings.json");
}

builder.Services.AddDbContext<FundooAppDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// ========================================
// 3. REPOSITORY REGISTRATION
// ========================================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<ILabelRepository, LabelRepository>();
builder.Services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();

// ========================================
// 4. SERVICE REGISTRATION
// ========================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<ILabelService, LabelService>();
builder.Services.AddScoped<ICollaboratorService, CollaboratorService>();

// ========================================
// 5. HELPER REGISTRATION
// ========================================
builder.Services.AddScoped<JwtTokenGenerator>();
builder.Services.AddScoped<OtpEmailSender>();

// ========================================
// 6. JWT AUTHENTICATION
// ========================================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

// Validate JWT Configuration
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is not configured. Please add 'Jwt:Key' to appsettings.json. " +
        "Example: \"Key\": \"MySecretKeyMinimum16Characters\"");
}

if (jwtKey.Length < 16)
{
    throw new InvalidOperationException(
        "JWT Key must be at least 16 characters long for security. " +
        $"Current length: {jwtKey.Length}");
}

if (string.IsNullOrEmpty(jwtIssuer))
{
    throw new InvalidOperationException(
        "JWT Issuer is not configured. Please add 'Jwt:Issuer' to appsettings.json");
}

if (string.IsNullOrEmpty(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT Audience is not configured. Please add 'Jwt:Audience' to appsettings.json");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

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
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ClockSkew = TimeSpan.Zero
    };
});

// ========================================
// 7. CORS POLICY
// ========================================
var corsOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FundooPolicy", policy =>
    {
        if (corsOrigins != null && corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
        else
        {
            // Fallback for development - allow all origins
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// ========================================
// 8. CONTROLLERS & API EXPLORER
// ========================================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ========================================
// 9. SWAGGER CONFIGURATION
// ========================================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Fundoo Notes API",
        Version = "v1",
        Description = "A comprehensive note-taking application API with authentication, collaboration, and labeling features",
        Contact = new OpenApiContact
        {
            Name = "Fundoo Notes",
            Email = "support@fundoonotes.com"
        }
    });

    // JWT Authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token. Example: Bearer eyJhbGc..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ========================================
// 10. LOGGING
// ========================================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// ========================================
// 11. CONFIGURATION VERIFICATION (DEBUG)
// ========================================
Console.WriteLine("\n" + new string('=', 60));
Console.WriteLine("🔍 CONFIGURATION VERIFICATION");
Console.WriteLine(new string('=', 60));
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Database: {connectionString?.Split(';')[0]}");
Console.WriteLine($"SMTP Host: {builder.Configuration["SmtpSettings:Host"]}");
Console.WriteLine($"SMTP From: {builder.Configuration["SmtpSettings:FromEmail"]}");
Console.WriteLine($"SMTP Port: {builder.Configuration["SmtpSettings:Port"]}");
Console.WriteLine($"SMTP SSL: {builder.Configuration["SmtpSettings:EnableSsl"]}");
Console.WriteLine($"JWT Issuer: {jwtIssuer}");
Console.WriteLine($"JWT Audience: {jwtAudience}");
Console.WriteLine($"JWT Key Length: {jwtKey.Length} characters");
Console.WriteLine($"CORS Origins: {(corsOrigins != null ? string.Join(", ", corsOrigins) : "None (Allow All)")}");
Console.WriteLine(new string('=', 60));
Console.WriteLine("✅ All configurations loaded successfully!");
Console.WriteLine(new string('=', 60) + "\n");

// ========================================
// 12. MIDDLEWARE PIPELINE
// ========================================

// Global Exception Handler
app.UseMiddleware<GlobalExceptionMiddleware>();

// Swagger (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Fundoo Notes API v1");
        options.RoutePrefix = string.Empty; // Swagger at root URL
    });
}

// HTTPS Redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("FundooPolicy");

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Controllers
app.MapControllers();

// ========================================
// 13. RUN APPLICATION
// ========================================
Console.WriteLine("🚀 Fundoo Notes API is running...");
Console.WriteLine($"📍 Swagger UI: {(app.Environment.IsDevelopment() ? "https://localhost:7014" : "Not available in production")}\n");

app.Run();