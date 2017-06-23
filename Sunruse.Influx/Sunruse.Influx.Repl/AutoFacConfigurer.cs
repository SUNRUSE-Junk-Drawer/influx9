using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunruse.Influx.Repl
{
    /// <summary>Configures a <see cref="ContainerBuilder"/> to use types from this assembly.</summary>
    public static class AutoFacConfigurer
    {
        /// <summary>Configures a <see cref="ContainerBuilder"/> to use types from this assembly.</summary>
        /// <param name="containerBuilder">The <see cref="ContainerBuilder"/> to configure.</param>
        public static void Configure(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterType<ConsoleReader>();
            containerBuilder.RegisterType<ConsoleWriter>();
        }
    }
}
