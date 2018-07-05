using System;
using System.Collections.Generic;
using System.Text;

namespace Orleans.SwaggerGen
{
    /// <summary>
    /// Parameter name of the Grain Key
    /// </summary>
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Method,  Inherited = true)]
    public  class GrainKeyAsNameAttribute: Attribute
    {
        public GrainKeyAsNameAttribute(string name)
        {
            this.Name = name;
        }
        public string Name { get; set; }
    }
}
