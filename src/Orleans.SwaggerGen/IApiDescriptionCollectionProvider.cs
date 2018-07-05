using System;
using System.Collections.Generic;
using System.Text;

namespace Orleans.SwaggerGen
{
    public interface IApiDescriptionCollectionProvider
    {
        IReadOnlyList<ApiDescription> Collection(string documentName);
    }
}
