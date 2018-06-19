using Microsoft.AspNetCore.Http;
using Orleans.HttpGateway.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orleans.HttpGateway.Core
{
    /// <summary>
    /// Resolve RouteData Get <see cref="GrainRouteValues"/>
    /// </summary>
    public interface IRouteDataResolve
    {
        GrainRouteValues Resolve(HttpContext context);
    }
}
