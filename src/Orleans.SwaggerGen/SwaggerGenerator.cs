using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orleans;
using Swashbuckle.AspNetCore.Swagger;
using Orleans.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Orleans.SwaggerGen
{
    public class SwaggerGenerator : ISwaggerProvider
    {
        private readonly SwaggerGeneratorSettings _settings;
        private readonly ISchemaRegistryFactory _schemaRegistryFactory;
        private readonly IApiDescriptionCollectionProvider _apiDescriptionCollectionProvider;

        public SwaggerGenerator(ISchemaRegistryFactory schemaRegistryFactory, IApiDescriptionCollectionProvider apiDescriptionCollectionProvider, SwaggerGeneratorSettings settings = null)
        {
            _settings = settings ?? new SwaggerGeneratorSettings();
            _schemaRegistryFactory = schemaRegistryFactory;
            _apiDescriptionCollectionProvider = apiDescriptionCollectionProvider;

        }
        public SwaggerDocument GetSwagger(string documentName, string host = null, string basePath = null, string[] schemes = null)
        {
            if (!_settings.SwaggerDocs.TryGetValue(documentName, out Info info))
                throw new UnknownSwaggerDocument(documentName);

            var schemaRegistry = _schemaRegistryFactory.Create();

            var apiDescriptions = _apiDescriptionCollectionProvider.Collection(documentName)
                .Where(apiDesc => _settings.DocInclusionPredicate(documentName, apiDesc))
                .Where(apiDesc => !_settings.IgnoreObsoleteActions || !apiDesc.IsObsolete()); ;

            //Generate SwaggerDocument
            var swaggerDoc = new SwaggerDocument
            {
                Info = info,
                Host = host,
                BasePath = basePath,
                Schemes = schemes,
                Paths = CreatePathItems(apiDescriptions, schemaRegistry),
                Definitions = schemaRegistry.Definitions,
                SecurityDefinitions = _settings.SecurityDefinitions.Any() ? _settings.SecurityDefinitions : null,
                Security = _settings.SecurityRequirements.Any() ? _settings.SecurityRequirements : null
            };

            var filterContext = new DocumentFilterContext(apiDescriptions,schemaRegistry);

            foreach (var filter in _settings.DocumentFilters)
            {
                filter.Apply(swaggerDoc, filterContext);
            }

            return swaggerDoc;
        }

        private Dictionary<string, PathItem> CreatePathItems(IEnumerable<ApiDescription> methods, ISchemaRegistry schemaRegistry)
        {
            return methods
                      .OrderBy(_settings.SortKeySelector)
                      .GroupBy(f => _settings.PathNameRelative(f))
                      .ToDictionary(group => group.Key, group => CreatePathItem(group, schemaRegistry));
        }

        private PathItem CreatePathItem(IEnumerable<ApiDescription> methods, ISchemaRegistry schemaRegistry)
        {
            var pathItem = new PathItem();
            pathItem.Post = CreateOperation(methods.FirstOrDefault(), schemaRegistry);
            return pathItem;
        }

        private Operation CreateOperation(ApiDescription apiDescription, ISchemaRegistry schemaRegistry)
        {
            var customAttributes = apiDescription.MethodInfo.GetCustomAttributes(true)
                   .Union(apiDescription.Type.GetTypeInfo().GetCustomAttributes(true));

            var isDeprecated = customAttributes.Any(attr => attr.GetType() == typeof(ObsoleteAttribute));

            var operation = new Operation
            {
                Tags = new[] { _settings.TagSelector(apiDescription) },
                OperationId = apiDescription.Name,
                Consumes = apiDescription.RelativeRequestMediaTypes(),
                Produces = apiDescription.RelativeResponseMediaTypes(),
                Parameters = CreateParameters(apiDescription, schemaRegistry),
                Responses = CreateResponses(apiDescription, schemaRegistry),
                Deprecated = isDeprecated ? true : (bool?)null
            };


            var filterContext = new OperationFilterContext(
               apiDescription,
               schemaRegistry,
               apiDescription.MethodInfo);

            foreach (var filter in _settings.OperationFilters)
            {
                filter.Apply(operation, filterContext);
            }

            return operation;
        }

        private IList<IParameter> CreateParameters(ApiDescription apiDescription, ISchemaRegistry schemaRegistry)
        {
            return apiDescription.Parameters
                    .Select(param => this.CreateParameter(param, schemaRegistry))
                    .ToList();
        }

        private IParameter CreateParameter(ParameterInfo parameterInfo, ISchemaRegistry schemaRegistry)
        {
            var customAttributes = parameterInfo.GetCustomAttributes(true);
            var isRequired = customAttributes.Any(attr => new[] { typeof(RequiredAttribute), typeof(BindRequiredAttribute) }.Contains(attr.GetType()));
            var schema = (parameterInfo.ParameterType != null) ? schemaRegistry.GetOrRegister(parameterInfo.ParameterType) : null;
            var name = _settings.DescribeAllParametersInCamelCase ? parameterInfo.Name.ToCamelCase() : parameterInfo.Name;


            //Determine if it is a simple type
            IParameter parameter;
            if (parameterInfo.ParameterType.IsSimpleType())
            {
                parameter= this.CreateNonBodyParameter(name, parameterInfo, schema, schemaRegistry);
            }
            else
            {
                parameter = new BodyParameter { Name = name, Schema = schema, Required = isRequired };
            }

            return parameter;
        }

        private IParameter CreateNonBodyParameter(string name, ParameterInfo param, Schema schema, ISchemaRegistry schemaRegistry)
        {
            var nonBodyParam = new NonBodyParameter
            {
                Name = name,
                In = "query",
            };

            if(param is GrainKeyParamterInfo)
            {
                nonBodyParam.In = "path";
                nonBodyParam.Required = true;
            }
            else
            {
                nonBodyParam.Required = param.HasDefaultValue;
            }

            if (schema == null)
            {
                nonBodyParam.Type = "string";
            }
            else
            {
                if (schema.Ref != null)
                {
                    // It's a referenced Schema and therefore needs to be located. This also means it's not neccessarily
                    // exclusive to this parameter and so, we can't assign any parameter specific attributes or metadata.
                    schema = schemaRegistry.Definitions[schema.Ref.Replace("#/definitions/", string.Empty)];
                }
                else
                {
                    // It's a value Schema. This means it's exclusive to this parameter and so, we can assign
                    // parameter specific attributes and metadata.
                    // Yep, it's hacky and needs to be refactored - SchemaRegistry should be stateless
                    schema.AssignAttributeMetadata(param.GetCustomAttributes(true));
                    schema.Default = (param != null && param.IsOptional)
                        ? param.DefaultValue
                        : null;
                }

                nonBodyParam.PopulateFrom(schema);
            }



            return nonBodyParam;
        }

        private IDictionary<string, Response> CreateResponses(ApiDescription apiDescription, ISchemaRegistry schemaRegistry)
        {
            var type = apiDescription.ReturnParameter.ParameterType.IsConstructedGenericType ?
                apiDescription.ReturnParameter.ParameterType.GenericTypeArguments.FirstOrDefault() : typeof(void);
            var supportedApiResponseTypes = new List<ApiResponseType>()
            {
                new ApiResponseType { StatusCode = "200",Type=type }
            };
            return supportedApiResponseTypes
              .ToDictionary(
                  apiResponseType => apiResponseType.StatusCode.ToString(),
                  apiResponseType => CreateResponse(apiResponseType, schemaRegistry));
        }

        private Response CreateResponse(ApiResponseType apiResponseType, ISchemaRegistry schemaRegistry)
        {
            var description = ResponseDescriptionMap
              .FirstOrDefault((entry) => Regex.IsMatch(apiResponseType.StatusCode.ToString(), entry.Key))
              .Value;

            return new Response
            {
                Description = description,
                Schema = (apiResponseType.Type != null && apiResponseType.Type != typeof(void))
                    ? schemaRegistry.GetOrRegister(apiResponseType.Type)
                    : null
            };
        }

        private static readonly Dictionary<string, string> ResponseDescriptionMap = new Dictionary<string, string>
        {
            { "1\\d{2}", "Information" },
            { "2\\d{2}", "Success" },
            { "3\\d{2}", "Redirect" },
            { "400", "Bad Request" },
            { "401", "Unauthorized" },
            { "403", "Forbidden" },
            { "404", "Not Found" },
            { "405", "Method Not Allowed" },
            { "406", "Not Acceptable" },
            { "408", "Request Timeout" },
            { "409", "Conflict" },
            { "4\\d{2}", "Client Error" },
            { "5\\d{2}", "Server Error" }
        };
    }

}
