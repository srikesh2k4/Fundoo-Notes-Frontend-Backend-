using BusinessLayer.Interfaces.Services;
using BusinessLayer.Services;
using DataBaseLayer.Context;
using DataBaseLayer.Interfaces;
using DataBaseLayer.Repositories;
using FundooNotes.Helpers;
using Microsoft.EntityFrameworkCore;

namespace FundooNotes.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<FundooAppDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                        sqlOptions.CommandTimeout(30);
                    });

                // Enable sensitive data logging only in development
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }
            });

            // Register Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
            services.AddScoped<ILabelRepository, LabelRepository>();
            services.AddScoped<ICollaboratorRepository, CollaboratorRepository>();

            // Register Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<INoteService, NoteService>();
            services.AddScoped<ILabelService, LabelService>();
            services.AddScoped<ICollaboratorService, CollaboratorService>();

            // Register Helpers
            services.AddScoped<JwtTokenGenerator>();
            services.AddScoped<OtpEmailSender>();

            // Add Memory Cache
            services.AddMemoryCache();

            // Add HTTP Context Accessor
            services.AddHttpContextAccessor();

            return services;
        }

        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Fundoo Notes API",
                    Version = "v1",
                    Description = "A comprehensive note-taking application with real-time collaboration features",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Name = "Fundoo Notes Support",
                        Email = "support@fundoonotes.com",
                        Url = new Uri("https://fundoonotes.com")
                    },
                    License = new Microsoft.OpenApi.Models.OpenApiLicense
                    {
                        Name = "MIT License",
                        Url = new Uri("https://opensource.org/licenses/MIT")
                    }
                });

                // Add JWT Authentication to Swagger
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by a space and your JWT token. Example: 'Bearer eyJhbGc...'",
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Enable XML comments (optional)
                // var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                // var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                // options.IncludeXmlComments(xmlPath);
            });

            return services;
        }

        public static IServiceCollection AddCustomCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });

                options.AddPolicy("Production", builder =>
                {
                    builder.WithOrigins(
                            "https://fundoonotes.com",
                            "https://www.fundoonotes.com")
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials();
                });
            });

            return services;
        }
    }
}