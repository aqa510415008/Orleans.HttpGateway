using Microsoft.AspNetCore.Http;
using Orleans.HttpGateway.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orleans.HttpGateway.Core
{
    /// <summary>
    /// Grain Prvoider
    /// </summary>
    public interface IGrainBuilder
    {
        /// <summary>
        /// Get Grain <see cref="GrainReference"/>
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        GrainReference GetGrain(GrainRouteValues route);
    }
}
