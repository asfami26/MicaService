using MicaService.Api.Jobs;
using MicaService.Api.Middlewares;
using MicaService.Application.Repositories;
using MicaService.Application.Services.Interfaces;
using MicaService.Infrastructure.Persistence;
using MicaService.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Quartz;
using MicaService.Api.Authentication;

namespace MicaService.Api;

public sealed class Startup(IConfiguration config)
{
    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
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
        services.AddSingleton<IClaimsTransformation, WindowsClaimsTransformation>();

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
                        .AllowCredentials()
                        .WithExposedHeaders("X-Next-Cursor", "X-Prev-Cursor", "X-Total-Count");
                }
                else
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders("X-Next-Cursor", "X-Prev-Cursor", "X-Total-Count");
                }
            });
        });

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = NegotiateDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = NegotiateDefaults.AuthenticationScheme;
            options.DefaultScheme = NegotiateDefaults.AuthenticationScheme;
        });

        if (env.IsDevelopment())
        {
            // DEV ONLY (Linux/local): bypass Windows/Kerberos and use DevAuth:UserName.
            // Keep this block for local dev; production on Windows IIS must use Negotiate.
            authBuilder.AddScheme<DevAuthenticationOptions, DevAuthenticationHandler>(
                NegotiateDefaults.AuthenticationScheme,
                options =>
                {
                    options.UserName = config.GetValue<string>("DevAuth:UserName");
                });
        }
        else
        {
            authBuilder.AddNegotiate();
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

}
