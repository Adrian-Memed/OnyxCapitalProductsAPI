using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProductsWebAPI.Helper;
using ProductsWebAPI.Interfaces;
using ProductsWebAPI.Middleware;
using ProductsWebAPI.Repository;
using ProductsWebAPI.Services;
using ProductsWebAPI.Validation;
using System.Data;
using System.Text;

namespace ProductsWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            string? userId = builder.Configuration["DatabaseCredentials:UserId"];
            string? password = builder.Configuration["DatabaseCredentials:Password"];
            string fullConnectionString = $"{connectionString};User ID={userId};Password={password};";

            // Load JWT settings from configuration
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];

            // Add JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        ValidateLifetime = true,  
                        ClockSkew = TimeSpan.Zero  
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<IDbConnection>(s =>
                new SqlConnection(fullConnectionString));

            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    connectionString: fullConnectionString,
                    healthQuery: "SELECT 1;",
                    name: "Sql Server Check",
                    failureStatus: HealthStatus.Unhealthy,
                    tags: new[] { "sql", "healthchecks" });

            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IDapperHelper, DapperHelper>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

            builder.Services.AddControllers();

            builder.Services.AddValidatorsFromAssemblyContaining<ProductValidator>();

            // Configure Swagger
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Products API", Version = "v1" });

                // Add JWT Bearer authentication to Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                // Apply the security scheme globally for all endpoints
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

            builder.Services.AddEndpointsApiExplorer();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.MapControllers();
            
            app.MapSwagger().RequireAuthorization();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            }).AllowAnonymous();

            app.Run();
        }
    }
}