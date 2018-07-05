using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Orleans.SwaggerGen.Test
{
    public class SwaggerGeneratorTest
    {
        [Fact]
        public void Generator()
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            var schemaRegistryFactory = new SchemaRegistryFactory(jsonSerializerSettings, new SchemaRegistrySettings());
            var stetings = new SwaggerGeneratorSettings();
            stetings.SwaggerDocs.Add("test", new Info()
            {
                Title = "≤‚ ‘",
                Version = "1.0.0"
            });
            stetings.ApiAssemblys.Add("test", typeof(IGer).Assembly);
            var grainDescriptionCollection = new ApiDescriptionCollectionProvider(stetings);

            SwaggerGenerator generator = new SwaggerGenerator( schemaRegistryFactory, grainDescriptionCollection, stetings);
            var swagger = generator.GetSwagger("test");


            JsonSerializer _swaggerSerializer= new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting =  Formatting.None,
                ContractResolver = new SwaggerContractResolver(jsonSerializerSettings)
            };

            var jsonBuilder = new StringBuilder();
            using (var writer = new StringWriter(jsonBuilder))
            {
                _swaggerSerializer.Serialize(writer, swagger);
            }
            string json = jsonBuilder.ToString();

         
        }
    }

    public interface GrainTest : IGrainWithIntegerKey
    {
        Task<string> Get(string name ="");
        Task<string> GetValue(UserInfo user);

        Task<Result> Add(UserInfo user);
    }

    [GrainKeyAsName("userId")]
    public class IGer : Grain, GrainTest
    {
        public Task<string> GetValue( UserInfo user)
        {
            throw new NotImplementedException();
        }

        public Task<Result> Add(UserInfo user)
        {
            throw new NotImplementedException();
        }
        public Task<string> Get(string name = "")
        {
            throw new NotImplementedException();
        }
    }

    public class UserInfo
    {
        [Required]
        public string Nick { get; set; }

        public string Name { get; set; }
    }

    public class Result
    {
        public string Code { get; set; }

        public string Msg { get; set; }
    }
}
