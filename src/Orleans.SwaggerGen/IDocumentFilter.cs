using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;

namespace Orleans.SwaggerGen
{
    public interface IDocumentFilter
    {
        void Apply(SwaggerDocument swaggerDoc, DocumentFilterContext context);
    }

    public class DocumentFilterContext
    {
        public DocumentFilterContext(
            IEnumerable<ApiDescription> apiDescriptions,
            ISchemaRegistry schemaRegistry)
        {
            ApiDescriptions = apiDescriptions;
            SchemaRegistry = schemaRegistry;
        }

        public IEnumerable<ApiDescription> ApiDescriptions { get; private set; }

        public ISchemaRegistry SchemaRegistry { get; private set; }
    }
}
