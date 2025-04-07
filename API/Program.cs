using API.Services;
using DataAccessLayer.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using System.Security.Claims;


namespace API
{
    public class Program
    { public static bool IsAuthenticated = false;
        public static void Main(string[] args)
        {
            
            var builder = WebApplication.CreateBuilder(args);

            // إضافة AppDbContext إلى DI
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // إعداد Authentication باستخدام JWT
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options =>
              {
                  options.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuer = true,
                      ValidateAudience = true,
                      ValidateLifetime = true,
                      ValidateIssuerSigningKey = true,
                      ValidIssuer = builder.Configuration["Jwt:Issuer"],
                      ValidAudience = builder.Configuration["Jwt:Audience"],
                      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                  }; options.Events = new JwtBearerEvents

                  {
                      OnAuthenticationFailed = context =>
                      {
                          if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                          {
                              context.Response.Headers.Add("Token-Expired", "true");
                          }
                          return Task.CompletedTask;
                      },
                      OnChallenge = async context =>
                      {
                          context.HandleResponse();

                          context.Response.StatusCode = 401;
                          await context.Response.WriteAsJsonAsync(new
                          {
                              error = "Unauthorized",
                              message = "Invalid or expired token."
                          });
                      }
                  };
              });
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("UserType", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        var userTypeClaim = context.User.FindFirst("UserType");

                        // إذا لم يتم إرسال UserType، يمكنك السماح أو رفض الوصول
                        if (userTypeClaim == null)
                        {
                            return false; // أو true إذا كنت تريد السماح عند غياب الـ UserType
                        }

                        // السماح فقط إذا كان UserType = 2
                        return int.TryParse(userTypeClaim.Value, out var userType) && userType == 2;
                    });
                });
            }); 
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("SameUserOrAdmin", policy =>
                {
                    policy.RequireAssertion(context =>
                    {
                        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
                        var userTypeClaim = context.User.FindFirst("UserType");

                        if (userIdClaim == null || userTypeClaim == null)
                        {
                            return false;
                        }

                        // السماح إذا كان UserType = 3
                        if (int.TryParse(userTypeClaim.Value, out var userType) && userType == 3)
                        {
                            return true;
                        }

                        // السماح إذا كان UserId يطابق الـ UserId في التوكن
                        var routeData = context.Resource as Microsoft.AspNetCore.Routing.RouteEndpoint;
                        var routeUserId = routeData?.RoutePattern.Defaults["id"]?.ToString();

                        return routeUserId != null && userIdClaim.Value == routeUserId;
                    });
                });
            });
            ;


            builder.Services.AddSwaggerGen();
            // إعداد Controllers مع معالجة الدوران
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

            // إضافة خدمات أخرى
            builder.Services.AddTransient<IEmailService, EmailService>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ILockoutService, LockoutService>();

            // إعداد Swagger
            builder.Services.AddEndpointsApiExplorer();

            // إعداد CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAny", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });



            var app = builder.Build();
            
            // ترتيب Middleware
            app.UseStaticFiles();
            app.UseDefaultFiles();

            app.UseRouting();

            app.UseCors("AllowAny");

            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                // Check if the request is for Swagger
                if (context.Request.Path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase))
                {
                    if (!IsAuthenticated)
                    {
                        // Restrict access if not authenticated
                        context.Response.StatusCode = 403; // Forbidden
                        await context.Response.WriteAsync("Access to Swagger is restricted.");
                        return;
                    }


                }

                // Continue to the next middleware
                await next();
            });

            // Always register Swagger so it can be accessed conditionally

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/swagger"))
                {
                    var timer = new Timer(state =>
                    {
                        IsAuthenticated = false; // Reset القيمة
                        Console.WriteLine("IsAuthenticated has been reset to false.");
                    }, null, 5000, Timeout.Infinite);
                }
                await next(); // Proceed to the next middleware

            });

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {

                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = "swagger";


            });
            app.MapControllers();

            app.Run();
        }
    }
}
