using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Orleans.HttpGateway.Configuration;
using Orleans.HttpGateway.Model;

namespace Orleans.HttpGateway.Core
{
    internal class DefaultRouteDataResolve : IRouteDataResolve
    {
        private readonly ConcurrentDictionary<string, Type> _GrainTypeCache = new ConcurrentDictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private readonly OrleansHttpGatewayOptions options;
        public DefaultRouteDataResolve(IOptions<OrleansHttpGatewayOptions> config)
        {
            this.options = config.Value;
        }
        public GrainRouteValues Resolve(HttpContext context)
        {
            var data = context.GetRouteData();
            string siloName = (string)data.Values["siloName"];
            string serviceName = (string)data.Values["serviceName"];
            string grainId = (string)data.Values["grainId"];
            string grainMethod = (string)data.Values["grainMethod"];

            Type type = _GrainTypeCache.GetOrAdd($"{siloName}.{serviceName}", (key) =>
             {
                 //Get client option based on serviceName
                 if (!options.Clients.ContainsKey(siloName))
                     throw new ArgumentNullException(nameof(OrleansClientOptions));
                 OrleansClientOptions option = options.Clients[siloName];
                 string grainInterface = option.InterfaceTemplate.Replace("{ServiceName}", serviceName);
                 return option.Assembly.ExportedTypes.Where(f => typeof(IGrain).IsAssignableFrom(f) && f.Name.Equals(grainInterface, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
             });
            if (type == null)
                throw new ArgumentNullException("Grain Type Not null");
            return new GrainRouteValues(context, siloName, serviceName, grainMethod, grainId, type);

        }
    }
}
