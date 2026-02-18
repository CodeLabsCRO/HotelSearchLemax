using System.Globalization;
using System.Text;
using HotelSearchLemax.Core.Interfaces;
using HotelSearchLemax.DataAccess;
using HotelSearchLemax.DataAccess.Data;
using HotelSearchLemax.Services.Implementations;
using HotelSearchLemax.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace HotelSearchLemax
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Set invariant culture for consistent decimal parsing (use "." instead of ",")
            var cultureInfo = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add API controllers
            builder.Services.AddControllers();

            // Add Swagger with JWT support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Hotel Search API",
                    Version = "v1",
                    Description = "A REST API for hotel search with ranking based on price and distance"
                });

                // Add JWT authentication to Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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

            // Add Authentication (Cookie for MVC, JWT for API)
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "MultiAuth";
                options.DefaultChallengeScheme = "MultiAuth";
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(1);
                options.SlidingExpiration = true;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
                };
            })
            .AddPolicyScheme("MultiAuth", "Cookie or JWT", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    // Use JWT for API requests
                    string? authorization = context.Request.Headers.Authorization;
                    if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    // Use JWT for /api paths
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }

                    // Use cookies for MVC/web requests
                    return CookieAuthenticationDefaults.AuthenticationScheme;
                };
            });

            // Add HttpContextAccessor for audit fields
            builder.Services.AddHttpContextAccessor();

            // Add DbContext
            builder.Services.AddDbContext<HotelDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add repositories and unit of work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Add services
            builder.Services.AddScoped<IHotelService, HotelService>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Configure request localization for invariant culture (decimal "." handling)
            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(CultureInfo.InvariantCulture);
                options.SupportedCultures = new[] { CultureInfo.InvariantCulture };
                options.SupportedUICultures = new[] { CultureInfo.InvariantCulture };
            });

            // Add health checks
            builder.Services.AddHealthChecks()
                .AddSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")!,
                    name: "database",
                    tags: ["db", "sql"]);

            var app = builder.Build();

            // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
                context.Database.Migrate();
                DbSeeder.Seed(context);
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotel Search API v1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRequestLocalization();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            // Map health check endpoint
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
