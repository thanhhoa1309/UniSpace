using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using UniSpace.Domain;
using UniSpace.Domain.Commons;
using UniSpace.Domain.Interfaces;
using UniSpace.Domain.Repository;
using UniSpace.Service.Interfaces;
using UniSpace.Service.Services;
using UniSpace.Services.Interfaces;
using UniSpace.Services.Services;

namespace UniSpace.Presentation.Architecture
{
    public static class IocContainer
    {
        public static IServiceCollection SetupIocContainer(this IServiceCollection services)
        {
            services.SetupDbContext();

            //Add generic repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            //Add business services
            services.SetupBusinessServicesLayer();

            services.SetupJwt();
            return services;
        }

        private static IServiceCollection SetupDbContext(this IServiceCollection services)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            // Get the connection string from "DefaultConnection"
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Register DbContext with PostgreSQL (Npgsql)
            services.AddDbContext<UniSpaceDbContext>(options =>
                options.UseNpgsql(connectionString,
                    sql => sql.MigrationsAssembly(typeof(UniSpaceDbContext).Assembly.FullName)
                )
            );

            return services;
        }

        public static IServiceCollection SetupBusinessServicesLayer(this IServiceCollection services)
        {
            // Inject service vào DI container
            services.AddHttpContextAccessor();

            // Register UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Domain Services
            services.AddScoped<IClaimsService, ClaimsService>();
            services.AddScoped<ICurrentTime, CurrentTime>();

            // Register Business Services
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICampusService, CampusService>();
            services.AddScoped<IRoomService, RoomService>();

            return services;
        }

        private static IServiceCollection SetupJwt(this IServiceCollection services)
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddEnvironmentVariables()
                .Build();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    x.SaveToken = true;
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,   // Bật kiểm tra Issuer
                        ValidateAudience = true, // Bật kiểm tra Audience
                        ValidateLifetime = true,
                        ValidIssuer = configuration["JWT:Issuer"],
                        ValidAudience = configuration["JWT:Audience"],
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:SecretKey"] ??
                                                                            throw new InvalidOperationException())),
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = ClaimTypes.Name,
                        RoleClaimType = ClaimTypes.Role
                    };
                    x.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Read from Session
                            var token = context.HttpContext.Session.GetString("AuthToken");

                            // For SignalR: read token from query string
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                            {
                                context.Token = accessToken;
                            }
                            else if (!string.IsNullOrEmpty(token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("StudentPolicy", policy =>
                    policy.RequireRole("Student"));

                options.AddPolicy("LecturerPolicy", policy =>
                    policy.RequireRole("Lecturer"));

                options.AddPolicy("AdminPolicy", policy =>
                    policy.RequireRole("Admin"));
            });

            return services;
        }
    }

}