using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Orleans.HttpGateway.Core
{
    /// <summary>
    /// Parse HttpContext to get parameters and bind to Grain Methods
    /// </summary>
    public interface IParameterBinder
    {
        Task<object[]> BindParameters(ParameterInfo[] parameters, HttpRequest request);
    }
}