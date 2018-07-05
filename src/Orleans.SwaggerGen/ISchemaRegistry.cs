using System;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.Swagger;

namespace Orleans.SwaggerGen
{
    public interface ISchemaRegistry
    {
        Schema GetOrRegister(Type type);

        IDictionary<string, Schema> Definitions { get; }
    }
}
