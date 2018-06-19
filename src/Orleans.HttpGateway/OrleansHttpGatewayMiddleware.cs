using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.HttpGateway.Configuration;
using Orleans.HttpGateway.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orleans.HttpGateway.Core
{
    public class OrleansHttpGatewayMiddleware
    {
        private readonly IGrainBuilder _grainBuilder;
        private readonly IGrainMethodInvoker _grainInvoker;
        private readonly OrleansHttpGatewayOptions _options;
        private readonly JsonSerializer _serializer;
        private readonly RequestDelegate _next;
        private readonly IRouteDataResolve _routeDataResolve;

        public OrleansHttpGatewayMiddleware(RequestDelegate next,
            IOptions<OrleansHttpGatewayOptions> config,
            JsonSerializer serializer,
            IGrainBuilder grainBuilder,
            IGrainMethodInvoker grainMethodInvoker,
            IRouteDataResolve routeDataResolve)
        {
            if (grainBuilder == null) throw new ArgumentNullException(nameof(grainBuilder));
            if (config == null) throw new ArgumentNullException(nameof(config));

            this._routeDataResolve = routeDataResolve;

            this._options = config.Value;
            this._grainBuilder = grainBuilder;
            this._grainInvoker = grainMethodInvoker;
            _serializer = serializer;
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var route = this.ResplveRouteData(context);
            var grain = this._grainBuilder.GetGrain(route);
            var result = await _grainInvoker.Invoke(grain, route);

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            using (var writer = new StreamWriter(context.Response.Body))
            {
                _serializer.Serialize(writer, result);
                await writer.FlushAsync();
            }
        }

        private GrainRouteValues ResplveRouteData(HttpContext context)
        {
            var route = _routeDataResolve.Resolve(context);
            if (route != null)
                return route;
            else
                return null;
        }
    }
}