using Microsoft.AspNetCore.Http;
using Orleans.HttpGateway.Infrastructure;
using Orleans.HttpGateway.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Orleans.HttpGateway.Core
{
    internal class DynamicGrainMethodInvoker : IGrainMethodInvoker
    {
        private readonly IParameterBinder _parameterBinder;
        private readonly ConcurrentDictionary<string, List<ObjectMethodExecutor>> _cachedExecutors = new ConcurrentDictionary<string, List<ObjectMethodExecutor>>();

        public DynamicGrainMethodInvoker(IParameterBinder parameterBinder)
        {
            _parameterBinder = parameterBinder;

        }

        public async Task<object> Invoke(GrainReference grain, GrainRouteValues route)
        {
            var executors = _cachedExecutors.GetOrAdd($"{grain.GrainType.FullName}.{route.GrainMethod}",
                (key) =>
                {
                    //Get grainType IEnumerable<MethodInfo> 
                    var mis = ReflectionUtil.GetMethodsIncludingBaseInterfaces(grain.GrainType)
                        .Where(x => string.Equals(x.Name, route.GrainMethod, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (mis.Count <= 0)
                        throw new ArgumentNullException(nameof(MethodInfo));
                    List<ObjectMethodExecutor> _executors = new List<ObjectMethodExecutor>();
                    foreach (var mi in mis)
                    {
                        var exe = ObjectMethodExecutor.Create(mi, grain.GrainType.GetTypeInfo());
                        _executors.Add(exe);
                    }
                    _executors.Sort((x, y) =>
                        -x.MethodParameters.Count().CompareTo(y.MethodParameters.Count())
                    );
                    return _executors;
                });

            foreach (var executor in executors)
            {
                var parameters = await GetParameters(executor, route.HttpContext.Request);
                if (executor.MethodParameters.Count() == parameters.Length)
                    return await executor.ExecuteAsync(grain.Grain, parameters);
            }
            throw new InvalidOperationException("No suitable parameter binder found for request");


        }

        private async Task<object[]> GetParameters(ObjectMethodExecutor executor, HttpRequest request)
        {

            //short circuit if no parameters
            if (executor.MethodParameters == null || executor.MethodParameters.Length == 0)
            {
                return Array.Empty<object>();
            }
            // loop through binders, in order
            // first suitable binder wins
            // so the order of registration is important
            ExceptionDispatchInfo lastException = null;
            try
            {
                return await _parameterBinder.BindParameters(executor.MethodParameters, request);
            }
            catch (Exception ex)
            {
                // continue on next suitable binder
                // but keep the exception when no other suitable binders are found
                lastException = ExceptionDispatchInfo.Capture(ex);
            }

            lastException?.Throw();
            return Array.Empty<object>();

        }
    }


}