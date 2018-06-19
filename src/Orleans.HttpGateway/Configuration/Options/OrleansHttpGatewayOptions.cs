using System.Collections.Generic;

namespace Orleans.HttpGateway.Configuration
{
    public class OrleansHttpGatewayOptions
    {

        public Dictionary<string, OrleansClientOptions> Clients { get; set; } = new Dictionary<string, OrleansClientOptions>();
      
        public  int InitializeAttemptsBeforeFailing { get; set; } = 10;


    }

  
}