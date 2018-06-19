using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Orleans.HttpGateway.Infrastructure;
using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Orleans.HttpGateway.Core
{
    public class DefaultParameterBinder : IParameterBinder
    {
        readonly JsonSerializer _serializer;
        public DefaultParameterBinder(JsonSerializer serializer)
        {
            this._serializer = serializer;
        }
        public async Task<object[]> BindParameters(ParameterInfo[] parameters, HttpRequest request)
        {
            if (parameters == null || parameters.Length <= 0)
                return Array.Empty<object>();

            //Get HttpRequest Data
            IQueryCollection queryData = request.Query;
            JObject bodyData = await this.BodyToJObjectAsync(request);

            var result = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                try
                {
                    var param = parameters[i];
                    object value = null;
                    if (param.ParameterType.IsPrimitive || param.ParameterType == typeof(string))
                    {
                        value = this.BindPrimitiveType(param, queryData, bodyData);
                    }
                    else if (param.ParameterType.IsArray)
                    {
                        value = this.BindArrayType(param, queryData, bodyData);
                    }
                    else if (param.ParameterType.IsClass)
                    {
                        value = this.BindClassType(param, bodyData);
                    }
                    else
                        throw new NotSupportedException("Bind this data type temporarily");

                    if (value == null)
                        return new object[0];

                    result[i] = value;
                }
                catch (Exception ex)
                {
                    throw new NotSupportedException("Bind this parameters data failed",ex);
                }
            }

            return result;
        }

        public object BindPrimitiveType(ParameterInfo parameter, IQueryCollection queryData, JObject bodyData)
        {
            if (queryData.TryGetValue(parameter.Name, out StringValues value))
            {
                return Convert(value, parameter.ParameterType);
            }
            else if (bodyData.TryGetValue(parameter.Name, StringComparison.OrdinalIgnoreCase, out JToken qvalue))
            {
                return qvalue.ToObject(parameter.ParameterType, _serializer);
            }
            else
                return null;
        }

        public object BindArrayType(ParameterInfo parameter, IQueryCollection queryData, JObject bodyData)
        {
            if (queryData.TryGetValue(parameter.Name, out StringValues value))
            {
                var elementType = ReflectionUtil.GetAnyElementType(parameter.ParameterType);
                Array array = Array.CreateInstance(elementType, value.Count);
                for (int p = 0; p < value.Count; p++)
                {
                    array.SetValue(Convert(value[p], elementType), p);
                }
                return array;
            }
            else if (bodyData.HasValues && bodyData.TryGetValue(parameter.Name, StringComparison.OrdinalIgnoreCase, out JToken qvalue))
            {
                return qvalue.ToObject(parameter.ParameterType, _serializer);
            }
            else if (bodyData.HasValues)
            {
                return bodyData.ToObject(parameter.ParameterType, _serializer);
            }
            else
                return null;
        }

        public object BindClassType(ParameterInfo parameter, JObject bodyData)
        {
            if (bodyData.HasValues && bodyData.TryGetValue(parameter.Name, StringComparison.OrdinalIgnoreCase, out JToken qvalue))
            {
                return qvalue.ToObject(parameter.ParameterType, _serializer);
            }
            else if (bodyData.HasValues)
            {
                return bodyData.ToObject(parameter.ParameterType, _serializer);
            }
            else
                return null;
        }


        object Convert(string source, Type t)
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
            //fallback to json serializer..
            using (var jsonTextReader = new JsonTextReader(new StringReader(source)))
            {
                return _serializer.Deserialize(jsonTextReader, t);
            }
        }

        async Task<JObject> BodyToJObjectAsync(HttpRequest request)
        {
            var requestContentType = request.GetTypedHeaders().ContentType;
            if (requestContentType?.MediaType != "application/json")
                return new JObject();

            if (!request.Body.CanSeek)
            {
                // JSON.Net does synchronous reads. In order to avoid blocking on the stream, we asynchronously 
                // read everything into a buffer, and then seek back to the beginning. 
                request.EnableRewind();
            }
            request.Body.Seek(0L, SeekOrigin.Begin);

            // parse encoding
            // default to UTF8
            var encoding = request.GetTypedHeaders().ContentType.Encoding ?? Encoding.UTF8;

            using (var reader = new JsonTextReader(new StreamReader(request.Body, encoding)))
            {
                reader.CloseInput = false;
                return await JObject.LoadAsync(reader);
            }
        }
    }
}