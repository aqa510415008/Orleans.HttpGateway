using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orleans.HttpGateway.Model
{
    public class GrainRouteValues
    {
        public GrainRouteValues(HttpContext context,string siloName, string serviceName,string grainMethod,string grainId,Type type)
        {
            this.HttpContext = context;
            this.ServiceName = serviceName;
            this.GrainMethod = grainMethod;
            this.GrainId = grainId;
            this.SiloName = siloName;
            this.GrainType = type;
        }
        public HttpContext HttpContext { get;  }
        public string SiloName { get;  }
        public string ServiceName { get;  }
        public string GrainMethod { get; }
        public string GrainId { get;  }
        public Type GrainType { get; }


    }
}
