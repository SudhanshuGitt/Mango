using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace Mango.Services.ProductAPI.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static WebApplicationBuilder AddAppAuthentication(this WebApplicationBuilder builder)
        {
            // validating token 
            var secret = builder.Configuration.GetValue<string>("ApiSettings:Secret");
            var issuer = builder.Configuration.GetValue<string>("ApiSettings:Issuer");
            var audience = builder.Configuration.GetValue<string>("ApiSettings:Audience");

            var key = Encoding.ASCII.GetBytes(secret);

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            })
                // configure bearer token 
                .AddJwtBearer(x =>
                {
                    // validating token based on validatioon parameters
                    // we need to create the object and define what exactly we need to validate
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                    };
                });

            builder.Services.AddAuthorization();

            return builder;
        }

        public static WebApplicationBuilder AddSwaggerGenWithAuth(this WebApplicationBuilder builder)
        {
            // add authentication and authoriztion in swagger
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter the Bearer authorization string as following : `Bearer Generated-JWT-Token`",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"

                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference=new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id=  JwtBearerDefaults.AuthenticationScheme
                            }
                        },new string[]{}
                    }

                });
            });

            return builder;
        }
    }
}
