using Sunruse.Influx.Unit;
using System;
using Xunit;

namespace Sunruse.Influx.Repl.Unit
{
    public sealed class ConsoleWriterTests : ConfiguredTestKit
    {
        [Fact]
        public void ImportsConsoleWriteLine()
        {
            var actor = ActorOfAsTestActorRef<ConsoleWriter>();

            Assert.Equal(Console.Write, actor.UnderlyingActor.ConsoleWrite);
        }

        [Fact]
        public void CallsConsoleWriteLineOncePerMessage()
        {
            var actor = ActorOfAsTestActorRef<ConsoleWriter>();
            var calls = 0;
            Action<string> consoleWriteLine = str => calls++;
            actor.UnderlyingActor.ConsoleWrite = consoleWriteLine;

            actor.Tell(new WriteToConsole("test request"));
            ExpectMsg<WrittenToConsole>();
            ExpectNoMsg();
        }

        [Fact]
        public void WritesTheSentLine()
        {
            var actor = ActorOfAsTestActorRef<ConsoleWriter>();
            string written = null;
            Action<string> consoleWriteLine = str => written = str;
            actor.UnderlyingActor.ConsoleWrite = consoleWriteLine;

            actor.Tell(new WriteToConsole("test request"));
            ExpectMsg<WrittenToConsole>();

            Assert.Equal("test request", written);
        }

        [Fact]
        public void ReportsItselfAsTheSender()
        {
            var actor = ActorOfAsTestActorRef<ConsoleWriter>();
            Action<string> consoleWriteLine = str => { };
            actor.UnderlyingActor.ConsoleWrite = consoleWriteLine;

            actor.Tell(new WriteToConsole("test request"));

            ExpectMsgFrom<WrittenToConsole>(actor);
        }
    }
}
