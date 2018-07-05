using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orleans.SwaggerGen
{
    public class ApiDescriptionCollectionProvider : IApiDescriptionCollectionProvider
    {
        private readonly SwaggerGeneratorSettings _settings;
        public ApiDescriptionCollectionProvider(SwaggerGeneratorSettings settings)
        {
            _settings = settings ?? new SwaggerGeneratorSettings();
        }
        public IReadOnlyList<ApiDescription> Collection(string documentName)
        {
            //Get all the methods in the Grain interface in the assembly
            if (!_settings.ApiAssemblys.TryGetValue(documentName, out Assembly assembly))
                throw new UnknownSwaggerDocument(documentName);

            var grainImpls = assembly.GetTypes()
                .Where(type => typeof(Grain).IsAssignableFrom(type) && type.IsClass && !type.IsInterface && !type.IsAbstract && !type.IsGenericType && type.IsPublic)
                .ToList();

            return grainImpls.SelectMany(grain =>
             {
                 return grain
                  .GetInterfaces()
                  .Where(type => typeof(IGrain).IsAssignableFrom(type) && type.IsPublic && !type.IsGenericType && type.Module.Name != "Orleans.Core.Abstractions.dll")
                  .SelectMany(interfaceType =>
                  {
                      var grainKey = this.ResolveGrainKey(interfaceType);
                      return interfaceType.GetMethods().Select(method =>
                      {
                          MethodInfo implMethods = grain.GetMethod(method.Name, method.GetParameters().Select(f => f.ParameterType).ToArray());
                          return new ApiDescription(grainKey, interfaceType, method, implMethods);
                      });
                  });
             }).ToList();
        }


        private GrainKeyParamterInfo ResolveGrainKey(Type type)
        {
            var grainKayNameAttr = type.GetCustomAttribute<GrainKeyAsNameAttribute>();
            var grainKeyName = grainKayNameAttr?.Name ?? "GrainKey";
            Type grainType;
            if (typeof(IGrainWithGuidCompoundKey).IsAssignableFrom(type) || typeof(IGrainWithGuidKey).IsAssignableFrom(type))
                grainType = typeof(Guid);
            else if (typeof(IGrainWithIntegerCompoundKey).IsAssignableFrom(type) || typeof(IGrainWithIntegerKey).IsAssignableFrom(type))
                grainType = typeof(long);
            else if (typeof(IGrainWithStringKey).IsAssignableFrom(type) || typeof(IGrainWithStringKey).IsAssignableFrom(type))
                grainType = typeof(string);
            else
                throw new Exception("不支持此类型");
            return new GrainKeyParamterInfo(grainKeyName, grainType);
        }


    }
}
