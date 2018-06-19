using Orleans.HttpGateway.Model;
using System;

namespace Orleans.HttpGateway.Core
{
    public interface IGrainReference
    {
        GrainReference GetGrainReference(Type type,GrainRouteValues grain);
    }


}