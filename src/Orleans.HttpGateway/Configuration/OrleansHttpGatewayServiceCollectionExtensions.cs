using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Orleans;
using Orleans.Configuration;
using Orleans.HttpGateway.Configuration;
using Orleans.HttpGateway.Core;
using Orleans.HttpGateway.Infrastructure;
using Orleans.Runtime;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OrleansHttpGatewayServiceCollectionExtensions
    {
        public static IServiceCollection AddOrleansHttpGateway(this IServiceCollection services, Action<OrleansHttpGatewayOptions> configure
            , Action<OrleansClientOptions, IClientBuilder> clientBuild)
        {
            services.AddRouting();
            services.TryAddSingleton<IGrainBuilder, DefaultGrainBuilder>();
            services.TryAddSingleton<IGrainFactoryGateway, DefaultGrainFactoryGateway>();
            services.TryAddSingleton<IGrainMethodInvoker, DynamicGrainMethodInvoker>();
            services.TryAddSingleton<IGrainReference, DefaultGrainReference>();
            services.TryAddSingleton<IRouteDataResolve, DefaultRouteDataResolve>();
            services.TryAddSingleton<IParameterBinder, DefaultParameterBinder>();

            services.TryAddSingleton(typeof(IKeyedServiceCollection<,>), typeof(KeyedServiceCollection<,>));

            //JsonSerializer 
            services.AddSingleton<JsonSerializer>((IServiceProvider serviceProvider) =>
            {
                JsonSerializerSettings settings = serviceProvider.GetService<JsonSerializerSettings>()
                    ?? new JsonSerializerSettings();

                if (!settings.Converters.OfType<ImmutableConverter>().Any())
                {
                    settings.Converters.Add(new ImmutableConverter());
                }
                return JsonSerializer.Create(settings);
            });


            services.Configure<OrleansHttpGatewayOptions>(options =>
            {
                configure?.Invoke(options);
            });
            var option = new OrleansHttpGatewayOptions();
            configure?.Invoke(option);
            foreach (var item in option.Clients)
            {
                var clientOption = item.Value;
                services.AddTransientNamedService<IClientBuilder>(clientOption.Assembly.Location, (service, key) =>
                {
                    var build = new ClientBuilder();
                    build.Configure<ClusterOptions>(opt =>
                    {
                        opt.ClusterId = clientOption.ClusterId;
                        opt.ServiceId = clientOption.ServiceId;
                    });
                    clientBuild?.Invoke(clientOption, build);
                    return build;
                });
            }
            return services;
        }



    }
}
