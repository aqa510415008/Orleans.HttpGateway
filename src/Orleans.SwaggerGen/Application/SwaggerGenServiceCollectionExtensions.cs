using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Orleans.SwaggerGen;
using Swashbuckle.AspNetCore.Swagger;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SwaggerGenServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGen(
            this IServiceCollection services,
            Action<SwaggerGenOptions> setupAction)
        {
            var options = new SwaggerGenOptions();
            setupAction.Invoke(options);

            ISwaggerProvider swaggerProvider = options.CreateSwaggerProvider();
            services.AddSingleton<ISwaggerProvider>(swaggerProvider);
            return services;
        }

      
    }
}
