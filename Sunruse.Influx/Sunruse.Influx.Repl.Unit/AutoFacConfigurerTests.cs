using Autofac;
using System;
using Xunit;

namespace Sunruse.Influx.Repl.Unit
{
    public sealed class AutoFacConfigurerTests
    {
        [Fact]
        public void RegistersConsoleReader()
        {
            var containerBuilder = new ContainerBuilder();

            AutoFacConfigurer.Configure(containerBuilder);

            Assert.True(containerBuilder.Build().IsRegistered<ConsoleReader>());
        }

        [Fact]
        public void RegistersConsoleWriter()
        {
            var containerBuilder = new ContainerBuilder();

            AutoFacConfigurer.Configure(containerBuilder);

            Assert.True(containerBuilder.Build().IsRegistered<ConsoleWriter>());
        }
    }
}
