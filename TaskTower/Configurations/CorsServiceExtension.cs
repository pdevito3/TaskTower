namespace TaskTower.Configurations;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal static class CorsServiceExtension
{
    // public static IServiceCollection AddCorsService(this IServiceCollection services, string policyName, IWebHostEnvironment env)
    public static IServiceCollection AddCorsService(this IServiceCollection services, string policyName)
    {
        // if (env.IsDevelopment())
        // {
            services.AddCors(options =>
            {
                options.AddPolicy(policyName, builder =>
                    builder.SetIsOriginAllowed(_ => true)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithExposedHeaders("X-Pagination"));
            });
        // }
        // else
        // {
        //     //TODO update origins here with env vars or secret
        //     //services.AddCors(options =>
        //     //{
        //     //    options.AddPolicy(policyName, builder =>
        //     //        builder.WithOrigins(origins)
        //     //        .AllowAnyMethod()
        //     //        .AllowAnyHeader()
        //     //        .WithExposedHeaders("X-Pagination"));
        //     //});
        // }

        return services;
    }
}