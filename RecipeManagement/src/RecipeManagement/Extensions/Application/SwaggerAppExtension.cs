namespace RecipeManagement.Extensions.Application;

using RecipeManagement.Middleware;
using RecipeManagement.Services;
using Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Resources;
using Swashbuckle.AspNetCore.SwaggerUI;

public static class SwaggerAppExtension
{
    public static void UseSwaggerExtension(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env)
    {
        if (!env.IsEnvironment(Consts.Testing.FunctionalTestingEnvName))
        {
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "");
                config.DocExpansion(DocExpansion.None);
            });
        }
    }
}