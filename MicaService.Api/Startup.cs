using System.Security.Cryptography;
using MicaService.Api.Jobs;
using MicaService.Api.Middlewares;
using MicaService.Application.Repositories;
using MicaService.Application.Services.Interfaces;
using MicaService.Infrastructure.Persistence;
using MicaService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Quartz;

namespace MicaService.Api;

public sealed class Startup(IConfiguration config)
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddMemoryCache();

        services.AddScoped<ISectionDbContext, SectionDbContext>();
        services.AddScoped<ILocationDbContext, LocationDbContext>();
        services.AddScoped<IEmployeeProfileDbContext, EmployeeProfileDbContext>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<ISectionHttpService, SectionHttpService>();
        services.AddScoped<ILocationService, LocationService>();
        services.AddScoped<ILocationHttpService, LocationHttpService>();
        services.AddScoped<IEmployeeProfileService, EmployeeProfileService>();
        services.AddScoped<IEmployeeProfileHttpService, EmployeeProfileHttpService>();

        var cron = config["Quartz:SectionRefreshCron"] ?? "0 0 6,12,18 ? * MON-FRI *";
        var locationCron = config["Quartz:LocationRefreshCron"] ?? cron;
        services.AddQuartz(options =>
        {
            var jobKey = new JobKey("section-cache-refresh");
            options.AddJob<SectionCacheRefreshJob>(job => job.WithIdentity(jobKey));
            options.AddTrigger(trigger => trigger
                .ForJob(jobKey)
                .WithIdentity("section-cache-refresh-trigger")
                .WithCronSchedule(cron));

            var locationJobKey = new JobKey("location-cache-refresh");
            options.AddJob<LocationCacheRefreshJob>(job => job.WithIdentity(locationJobKey));
            options.AddTrigger(trigger => trigger
                .ForJob(locationJobKey)
                .WithIdentity("location-cache-refresh-trigger")
                .WithCronSchedule(locationCron));

            var employeeJobKey = new JobKey("employee-profile-cache-refresh");
            options.AddJob<EmployeeProfileCacheRefreshJob>(job => job.WithIdentity(employeeJobKey));
            options.AddTrigger(trigger => trigger
                .ForJob(employeeJobKey)
                .WithIdentity("employee-profile-cache-refresh-trigger")
                .WithCronSchedule(cron));
        });
        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });

        var allowedOrigins = config.GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();
        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                if (allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
                else
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        var jwt = config.GetSection("Jwt");
        var publicKeyPath = jwt["PublicKeyPath"];
        var privateKeyPath = jwt["PrivateKeyPath"];

        if (!string.IsNullOrWhiteSpace(publicKeyPath))
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(File.ReadAllText(publicKeyPath));
            var decryptionRsa = !string.IsNullOrWhiteSpace(privateKeyPath)
                ? LoadRsa(privateKeyPath)
                : null;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.MapInboundClaims = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt["Issuer"],
                        ValidAudience = jwt["Audience"],
                        IssuerSigningKey = new RsaSecurityKey(rsa),
                        TokenDecryptionKey = decryptionRsa is not null
                            ? new RsaSecurityKey(decryptionRsa)
                            : null,
                        RoleClaimType = "role",
                        NameClaimType = "username"
                    };
                    o.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            if (context.Request.Cookies.TryGetValue("access_token", out var token))
                            {
                                context.Token = token;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
        }

        services.AddAuthorization();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Mica Service API",
                Version = "v1",
                Description = "Mica Department Cache Service"
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Input JWT token: Bearer {token}"
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
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mica Service API v1");
            });
        }

        app.UseRouting();
        app.UseCors("Frontend");
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }

    private static RSA LoadRsa(string path)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(path));
        return rsa;
    }
}

