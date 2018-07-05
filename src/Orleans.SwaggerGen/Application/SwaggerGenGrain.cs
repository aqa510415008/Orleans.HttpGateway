using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.SwaggerGen
{
    public class SwaggerGenGrain :Grain, ISwaggerGenGrain
    {
        private readonly ISwaggerProvider swaggerProvider;
        public SwaggerGenGrain(ISwaggerProvider swaggerProvider)
        {
            this.swaggerProvider = swaggerProvider;
        }
        public Task<string> Generator()
        {
            var jsonSerializerSettings = new JsonSerializerSettings();

            var swagger = swaggerProvider.GetSwagger("test");

            JsonSerializer _swaggerSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                ContractResolver = new SwaggerContractResolver(jsonSerializerSettings)
            };

            var jsonBuilder = new StringBuilder();
            using (var writer = new StringWriter(jsonBuilder))
            {
                _swaggerSerializer.Serialize(writer, swagger);
            }
            return Task.FromResult( jsonBuilder.ToString());
        }
    }
}
