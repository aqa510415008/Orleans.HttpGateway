using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Orleans.HttpGateway.Configuration;
using Orleans.HttpGateway.Core;
using System;

namespace Microsoft.AspNetCore.Builder
{
    public static class OrleansHttpGatewayApplicationBuilderExtensions
    {
      
        public static IApplicationBuilder UseOrleansHttpGateway(this IApplicationBuilder app)
        {
            var routeBuilder = new RouteBuilder(app);         
            routeBuilder.MapMiddlewareRoute("{siloName}/{serviceName}/{grainMethod}/{grainId?}", part =>
            {
                part.UseMiddleware<OrleansHttpGatewayMiddleware>();
            });
            var routes = routeBuilder.Build();

            app.UseRouter(routes);

            return app;
        }
    }

}