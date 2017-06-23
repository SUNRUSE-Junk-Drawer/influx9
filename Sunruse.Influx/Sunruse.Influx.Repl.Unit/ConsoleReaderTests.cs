using Sunruse.Influx.Unit;
using System;
using Xunit;

namespace Sunruse.Influx.Repl.Unit
{
    public sealed class ConsoleReaderTests : ConfiguredTestKit
    {
        [Fact]
        public void ImportsConsoleReadLine()
        {
            var actor = ActorOfAsTestActorRef<ConsoleReader>();

            Assert.Equal(Console.ReadLine, actor.UnderlyingActor.ConsoleReadLine);
        }

        [Fact]
        public void CallsConsoleReadLineOncePerMessage()
        {
            var actor = ActorOfAsTestActorRef<ConsoleReader>();
            var calls = 0;
            Func<string> consoleReadLine = () =>
            {
                calls++;
                return "test response";
            };
            actor.UnderlyingActor.ConsoleReadLine = consoleReadLine;

            actor.Tell(new ReadLineFromConsole());
            ExpectMsg<LineReadFromConsole>();
            ExpectNoMsg();

            Assert.Equal(1, calls);
        }

        [Fact]
        public void SendsTheReadLineBack()
        {
            var actor = ActorOfAsTestActorRef<ConsoleReader>();
            Func<string> consoleReadLine = () => "test response";
            actor.UnderlyingActor.ConsoleReadLine = consoleReadLine;

            actor.Tell(new ReadLineFromConsole());
            var response = ExpectMsg<LineReadFromConsole>();

            Assert.Equal("test response", response.Line);
        }

        [Fact]
        public void ReportsItselfAsTheSender()
        {
            var actor = ActorOfAsTestActorRef<ConsoleReader>();
            Func<string> consoleReadLine = () => "test response";
            actor.UnderlyingActor.ConsoleReadLine = consoleReadLine;

            actor.Tell(new ReadLineFromConsole());

            ExpectMsgFrom<LineReadFromConsole>(actor);
        }
    }
}
