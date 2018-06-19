using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans.HttpGateway.Configuration;
using Orleans.Runtime;

namespace Orleans.HttpGateway.Core
{
    public class DefaultGrainFactoryGateway : IGrainFactoryGateway
    {
        private readonly ConcurrentDictionary<string, IClusterClient> clientsCache = new ConcurrentDictionary<string, IClusterClient>();
        private readonly ILogger logger;
        private readonly OrleansHttpGatewayOptions options;
        private readonly IServiceProvider serviceProvider;

        public DefaultGrainFactoryGateway(IServiceProvider serviceProvider,IOptions<OrleansHttpGatewayOptions> config, ILogger<DefaultGrainFactoryGateway> logger)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            this.logger = logger;
            this.options = config.Value;
            this.serviceProvider = serviceProvider;
        }
        public void BindGrainReference(IAddressable grain)
        {
            grain.BindGrainReference(this);
        }

        public Task<TGrainObserverInterface> CreateObjectReference<TGrainObserverInterface>(IGrainObserver obj) where TGrainObserverInterface : IGrainObserver
        {
            throw new NotImplementedException();
        }

        public Task DeleteObjectReference<TGrainObserverInterface>(IGrainObserver obj) where TGrainObserverInterface : IGrainObserver
        {
            throw new NotImplementedException();
        }

        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithGuidKey
        {
          return  this.GetClusterClient<TGrainInterface>().GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithIntegerKey
        {
            Type a = typeof(TGrainInterface);
            return this.GetClusterClient<TGrainInterface>().GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(string primaryKey, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithStringKey
        {
            return this.GetClusterClient<TGrainInterface>().GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string keyExtension, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithGuidCompoundKey
        {
            return this.GetClusterClient<TGrainInterface>().GetGrain<TGrainInterface>(primaryKey, keyExtension, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string keyExtension, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithIntegerCompoundKey
        {
            return this.GetClusterClient<TGrainInterface>().GetGrain<TGrainInterface>(primaryKey, keyExtension,grainClassNamePrefix);
        }

        /// <summary>
        /// 获取Orleans ClusterClient
        /// </summary>
        /// <typeparam name="TGrainInterface"></typeparam>
        private IClusterClient GetClusterClient<TGrainInterface>()
        {
            IClusterClient client = null;
            string name = typeof(TGrainInterface).Assembly.Location;

            int attempt = 0;
            while (true)
            {
                try
                {
                    client = clientsCache.GetOrAdd(name, (key) =>
                    {
                        return BuilderClient(name);
                    });
                    if (client.IsInitialized)
                        return client;
                    else
                    {
                        lock (client)
                        {
                            //客户端未初始化，连接服务端
                            client.Connect().Wait();
                            logger.LogDebug($"Connection {name} Sucess...");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    attempt++;
                    if (attempt <= this.options.InitializeAttemptsBeforeFailing)
                    {
                        client = clientsCache.GetOrAdd(name, (key) =>
                        {
                            return BuilderClient(name);
                        });

                        logger.LogDebug($"Attempt {attempt} of " + this.options.InitializeAttemptsBeforeFailing + " failed to initialize the Orleans client.");
                        Task.Delay(TimeSpan.FromSeconds(4)).Wait();
                        continue;
                    }
                    logger.LogError($"Connection {name} Faile...");
                    throw new Exception($"Connection {name} Faile...");
                }
                return client;
            }
        }

        private IClusterClient BuilderClient(string name)
        {
            IClientBuilder builder = this.serviceProvider.GetRequiredServiceByName<IClientBuilder>(name);
            return builder.Build();
        }

  
    }
}
