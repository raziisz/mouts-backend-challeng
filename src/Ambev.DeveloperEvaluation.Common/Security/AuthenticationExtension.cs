using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace Ambev.DeveloperEvaluation.Common.Security
{
    public static class AuthenticationExtension
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            var secretKey = configuration["Jwt:SecretKey"]?.ToString();
            ArgumentException.ThrowIfNullOrWhiteSpace(secretKey);

            var key = Encoding.ASCII.GetBytes(secretKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                x.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        await WriteErrorResponseAsync(
                            context.HttpContext,
                            StatusCodes.Status401Unauthorized,
                            "AuthenticationError",
                            "Authentication failed",
                            "A valid authentication token is required.");
                    },
                    OnForbidden = async context =>
                    {
                        await WriteErrorResponseAsync(
                            context.HttpContext,
                            StatusCodes.Status403Forbidden,
                            "AuthorizationError",
                            "Access denied",
                            "You do not have permission to access this resource.");
                    }
                };
            });

            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }

        private static async Task WriteErrorResponseAsync(
            HttpContext context,
            int statusCode,
            string type,
            string error,
            string detail)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                type,
                error,
                detail
            });
        }
    }
}
