using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Orleans.SwaggerGen
{
  public  class GrainKeyParamterInfo: ParameterInfo
    {
        public GrainKeyParamterInfo(string name,Type type )
        {
            this.NameImpl = name;
            this.ClassImpl = type;
        }
    }
}
