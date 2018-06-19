using Shouldly;
using System;
using Xunit;

namespace Orleans.HttpGateway.Tests
{
    public class AssemblyBasedGrainTypeProviderTests
    {
        [Theory
        ,InlineData("Orleans.HttpGateway.Tests.ITestGrain1", typeof(ITestGrain1))
        ,InlineData("Orleans.HttpGateway.Tests.ITestGrain2", typeof(ITestGrain2))
        ,InlineData("Orleans.HttpGateway.Tests.ITestGrain3", typeof(ITestGrain3))
        ,InlineData("Orleans.HttpGateway.Tests.ITestGrainNONEXISTENT", null)]
        public void Can_provide_TestGrainInterfaces(string name, Type excpected)
        {
            var sut = new AssemblyBasedGrainTypeProvider(typeof(AssemblyBasedGrainTypeProviderTests).Assembly);

            sut.GetGrainType(name).ShouldBe(excpected);
        }
    }
}