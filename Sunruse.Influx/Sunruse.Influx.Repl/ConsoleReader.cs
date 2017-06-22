using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunruse.Influx.Repl
{
    /// <summary>Wraps <see cref="Console.ReadLine"/>.</summary>
    public sealed class ConsoleReader : ReceiveActor
    {
        /// <summary>A reference to <see cref="Console.ReadLine"/>, which can be replaced for mocking purposes.</summary>
        public Func<string> ConsoleReadLine = Console.ReadLine;

        /// <inheritdoc />
        public ConsoleReader()
        {
            Receive<ReadLineFromConsole>(rlfc => Sender.Tell(new LineReadFromConsole(ConsoleReadLine())));
        }
    }

    /// <summary>A request to <see cref="ConsoleReader"/> to read a line from the console.</summary>
    public sealed class ReadLineFromConsole { }

    /// <summary>Returned from <see cref="ConsoleReader"/> in response to <see cref="ReadLineFromConsole"/>.</summary>
    public sealed class LineReadFromConsole
    {
        /// <summary>The line read from the console.</summary>
        public readonly string Line;

        /// <inheritdoc />
        /// <param name="line"><see cref="Line"/>.</param>
        public LineReadFromConsole(string line)
        {
            Line = line;
        }
    }
}
