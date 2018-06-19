using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orleans;
using Orleans.HttpGateway.Configuration;

namespace OrleansGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddOrleansHttpGateway(opt =>
            {
                opt.Clients.Add("api", new OrleansClientOptions()
                {
                    ClusterId = "dev",
                    ServiceId = "dev",
                    ClusterType = OrleansClusterType.StaticGateway,
                    InterfaceDllPathName = @"E:\工作项目\project_Zop\Components\Orleans.HttpGateway\example\OrleansHost\bin\Debug\netcoreapp2.1\OrleansInterface.dll",
                    ServiceName = "api",
                    InterfaceTemplate = "I{ServiceName}Service"
                });

            }, (option, build) =>
            {
                if(option.ClusterType == OrleansClusterType.StaticGateway)
                {
                    build.UseLocalhostClustering();
                }
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseOrleansHttpGateway();
            app.UseMvc();
        }
    }
}
