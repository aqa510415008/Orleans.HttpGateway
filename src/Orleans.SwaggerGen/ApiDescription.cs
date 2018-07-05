using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Orleans.SwaggerGen
{
    public class ApiDescription
    {
        public ApiDescription(GrainKeyParamterInfo grainKey, Type type, MethodInfo methodInfo, MethodInfo implMethod)
        {
            this.GrainKey = grainKey;
            this.Type = type;
            this.MethodInfo = methodInfo;
            this.ImplMethod = implMethod;
        }
        public string Name
        {
            get
            {
                return this.Type.Name + this.MethodInfo.Name;
            }
        }
        public GrainKeyParamterInfo GrainKey { get; }
        public Type Type { get; }
        public MethodInfo MethodInfo { get; }
        public MethodInfo ImplMethod { get; }
        public ParameterInfo ReturnParameter
        {
            get
            {
                return MethodInfo.ReturnParameter;
            }
        }
        public ParameterInfo[] Parameters
        {
            get
            {
                var parameters = MethodInfo.GetParameters().ToList();
                if (this.GrainKey.ParameterType != typeof(Guid))
                {
                    parameters.Insert(0, this.GrainKey);
                }
                return parameters.ToArray();
            }
        }
    }
}
