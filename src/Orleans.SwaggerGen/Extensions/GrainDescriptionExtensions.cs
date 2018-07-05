using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orleans.SwaggerGen
{
    internal static class GrainDescriptionExtensions
    {


        internal static IList<string> RelativeRequestMediaTypes(this ApiDescription method)
        {
            List<string> mediaTypes = new List<string>();
            mediaTypes.Add("application/json");
            return mediaTypes;
        }

        internal static IList<string> RelativeResponseMediaTypes(this ApiDescription method)
        {
            List<string> mediaTypes = new List<string>();
            mediaTypes.Add("application/json");
            return mediaTypes;
        }


        internal static bool IsObsolete(this ApiDescription apiDescription)
        {
            return apiDescription.MethodInfo.GetCustomAttributes(true)
                .Union(apiDescription.MethodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes(true))
                .Any(attr => attr.GetType() == typeof(ObsoleteAttribute));
        }
    }
}
