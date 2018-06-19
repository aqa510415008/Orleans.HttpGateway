using Orleans.HttpGateway.Infrastructure;
using Orleans.HttpGateway.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orleans.HttpGateway.Core
{

    internal class DefaultGrainReference : IGrainReference
    {
        private readonly IGrainFactoryGateway _grainFactory;
        private readonly Tuple<Type, MethodInfo>[] _grainIdentityInterfaceMap =
         typeof(IGrainFactory)
         .GetMethods()
             .Where(x => x.Name == "GetGrain" && x.IsGenericMethod)
             .Select(x => Tuple.Create(x.GetGenericArguments()[0].GetGenericParameterConstraints()[0], x)).ToArray();


        public DefaultGrainReference(IGrainFactoryGateway grainFactory)
        {
            _grainFactory = grainFactory;
        }

        public GrainReference GetGrainReference(Type type, GrainRouteValues route)
        {
            var grain = this.BuildFactoryMethod(type)(route.GrainId);
            return new GrainReference(type, grain);
        }

        private Func<string, object> BuildFactoryMethod(Type grainType)
        {
            var mi = _grainIdentityInterfaceMap.FirstOrDefault(x => x.Item1.IsAssignableFrom(grainType));
            if (mi != null)
            {
                var factoryDelegate =
                    DelegateFactory.Create(mi.Item2.GetGenericMethodDefinition().MakeGenericMethod(grainType));
                var idParser = GetArgumentParser(mi.Item2.GetParameters());
                return (id) => factoryDelegate(_grainFactory, idParser(id));
            }
            throw new NotSupportedException($"cannot construct grain {grainType.Name}");
        }

        private Func<string, object[]> GetArgumentParser(ParameterInfo[] parameters)
        {
            string[] idseperator = new[] { "," };

            return (id) =>
            {
                if (string.IsNullOrEmpty(id))
                {
                    if (parameters.Where(f => f.ParameterType == typeof(long)).Count() > 0)
                        id = "0";
                    else if (parameters.Where(f => f.ParameterType == typeof(Guid)).Count() > 0)
                        id = Guid.NewGuid().ToString();
                    else
                        id = "";
                }
                var idParts = id.Split(idseperator, StringSplitOptions.RemoveEmptyEntries);
                object[] values = new object[parameters.Length];
                for (int i = 0; i < idParts.Length; i++)
                {
                    values[i] = TryParse(idParts[i], parameters[i].ParameterType);
                }
                return values;
            };
        }
        static object TryParse(string source, Type t)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(t);
            if (converter.CanConvertTo(t) && converter.CanConvertFrom(typeof(string)))
            {
                return converter.ConvertFromString(source);
            }
            else if (t == typeof(Guid))
            {
                return Guid.Parse(source);
            }
            throw new ArgumentException($"Can't parse '{source}' as {t.FullName}", nameof(source));
        }



    }
}
