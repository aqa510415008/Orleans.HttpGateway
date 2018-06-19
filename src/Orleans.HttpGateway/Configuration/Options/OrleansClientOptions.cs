using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Orleans.HttpGateway.Configuration
{
    public class OrleansClientOptions
    {
        public string ServiceId { get; set; }
        public string ClusterId { get; set; }
        public string ServiceName { get; set; }
        /// <summary>
        /// Orleans Grain Interface path name
        /// </summary>
        public string InterfaceDllPathName { get; set; }
        public string InterfaceTemplate { get; set; }
        public bool IsAuthorizationBearer { get; set; } = false;
     
        public OrleansClusterType ClusterType { get; set; }
        public Dictionary<string,string> ClusterOptions { get; set; }
        internal Assembly Assembly
        {
            get
            {
                return Assembly.LoadFile(this.InterfaceDllPathName);
            }
        }
    }

    public enum OrleansClusterType
    {
        Consul,
        Zookeeper,
        StaticGateway
    }
}
