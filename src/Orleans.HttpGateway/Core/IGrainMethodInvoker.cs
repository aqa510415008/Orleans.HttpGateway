using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Orleans.HttpGateway.Model;

namespace Orleans.HttpGateway.Core
{
    public interface IGrainMethodInvoker
    {
        Task<object> Invoke(Model.GrainReference grain, GrainRouteValues route);
    }


}