using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Orleans.HttpGateway.Configuration;
using Orleans.HttpGateway.Model;

namespace Orleans.HttpGateway.Core
{
    internal class DefaultGrainBuilder : IGrainBuilder
    {
        private readonly ConcurrentDictionary<string, GrainReference> _GrainReferenceCache = new ConcurrentDictionary<string, GrainReference>(StringComparer.OrdinalIgnoreCase);
        private readonly IGrainReference _grainReferenceProvider;

        private readonly OrleansHttpGatewayOptions _options;
        public DefaultGrainBuilder(IGrainReference grainReferenceProvider, IOptions<OrleansHttpGatewayOptions> config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            this._grainReferenceProvider = grainReferenceProvider ?? throw new ArgumentNullException(nameof(IGrainReference));
            this._options = config.Value;
        }
        public GrainReference GetGrain(GrainRouteValues route)
        {
            var grain = _GrainReferenceCache.GetOrAdd(route.GrainType.FullName, key => this.GetGrainReference(route));
            var option = _options.Clients[route.SiloName];
            if ((bool)option?.IsAuthorizationBearer)
            {
                this.SetAuthorization(route.HttpContext);
            }
            return grain;
        }

        private GrainReference GetGrainReference(GrainRouteValues route)
        {
            return _grainReferenceProvider.GetGrainReference(route.GrainType, route);
        }


        /// <summary>
        /// 设置授权码
        /// </summary>
        /// <param name="accessType"></param>
        private void SetAuthorization(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue("Authorization", out StringValues value))
                Orleans.Runtime.RequestContext.Set("Authorization", string.Format("Bearer {0}", value));
        }
    }
}
