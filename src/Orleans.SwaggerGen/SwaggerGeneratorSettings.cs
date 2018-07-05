using Swashbuckle.AspNetCore.Swagger;
using Orleans.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Orleans.SwaggerGen
{
    public class SwaggerGeneratorSettings
    {
        public SwaggerGeneratorSettings()
        {
            SwaggerDocs = new Dictionary<string, Info>();
            DocInclusionPredicate = (docName, api) => api.Name != null;
            ApiAssemblys = new Dictionary<string, Assembly>();
            TagSelector = (api) => api.Type.Name;
            SortKeySelector = (api) => TagSelector(api);
            PathNameRelative = (api) =>
            {
                string grainKey = api.GrainKey.ParameterType == typeof(Guid) ? "" : "/{" + api.GrainKey.Name + "}";
                Regex rg = new Regex("(?<=(I))[.\\s\\S]*?(?=(Service))", RegexOptions.Multiline | RegexOptions.Singleline);
                string name = rg.Match(api.Type.Name).Value;
                return $"/{name}/{api.MethodInfo.Name}{grainKey}";
            };
            SecurityDefinitions = new Dictionary<string, SecurityScheme>();
            SecurityRequirements = new List<IDictionary<string, IEnumerable<string>>>();
            OperationFilters = new List<IOperationFilter>();
            DocumentFilters = new List<IDocumentFilter>();
        }
        public IDictionary<string, Info> SwaggerDocs { get; set; }
        public IDictionary<string, Assembly> ApiAssemblys { get; set; }
        public Func<string, ApiDescription, bool> DocInclusionPredicate { get; set; }
        public Func<ApiDescription, string> PathNameRelative { get; set; }
        public bool IgnoreObsoleteActions { get; set; }
        public Func<ApiDescription, string> TagSelector { get; set; }
        public Func<ApiDescription, string> SortKeySelector { get; set; }
        public IDictionary<string, SecurityScheme> SecurityDefinitions { get; private set; }
        public IList<IDictionary<string, IEnumerable<string>>> SecurityRequirements { get; private set; }
        public bool DescribeAllParametersInCamelCase { get; set; }
        public IList<IOperationFilter> OperationFilters { get; private set; }
        public IList<IDocumentFilter> DocumentFilters { get; private set; }
    }
}
