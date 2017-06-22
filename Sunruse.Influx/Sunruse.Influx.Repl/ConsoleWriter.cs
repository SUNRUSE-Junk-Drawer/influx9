using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunruse.Influx.Repl
{
    /// <summary>Wraps <see cref="Console.Write(string)"/>.</summary>
    public sealed class ConsoleWriter : ReceiveActor
    {
        /// <summary>A reference to <see cref="Console.Write(string)"/>, which can be replaced for mocking purposes.</summary>
        public Action<string> ConsoleWrite = Console.Write;

        /// <inheritdoc />
        public ConsoleWriter()
        {
            Receive<WriteToConsole>(wltc =>
            {
                ConsoleWrite(wltc.Line);
                Sender.Tell(new WrittenToConsole());
            });
        }
    }

    /// <summary>A request to <see cref="ConsoleWriter"/> to read a line from the console.</summary>
    public sealed class WriteToConsole
    {
        /// <summary>The line to write to the console.</summary>
        public readonly string Line;

        /// <inheritdoc />
        /// <param name="line"><see cref="Line"/>.</param>
        public WriteToConsole(string line)
        {
            Line = line;
        }
    }

    /// <summary>Returned from <see cref="ConsoleWriter"/> in response to <see cref="WriteToConsole"/>.</summary>
    public sealed class WrittenToConsole { }
}
