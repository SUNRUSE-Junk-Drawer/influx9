using Akka.Actor;
using Akka.DI.AutoFac;
using Akka.DI.Core;
using Autofac;
using System;
using System.Collections.Generic;
using Xunit;

namespace Sunruse.Influx.Unit
{
    public sealed class ActorLookupTests : ConfiguredTestKit
    {
        public sealed class RefWrapper
        {
            public IActorRef Ref;
        }

        public sealed class Start { }

        public sealed class SelfTestActor : ReceiveActor
        {
            public SelfTestActor()
            {
                Receive<Start>(s => Sender.Tell(new RefWrapper { Ref = new ActorLookup().Self(Context) }));
            }
        }

        [Fact]
        public void SelfReturnsReferenceToSelf()
        {
            var actor = ActorOf<SelfTestActor>();

            actor.Tell(new Start());

            ExpectMsg<RefWrapper>(ar => ar.Ref == actor);
        }

        public sealed class ParentTestActor : ReceiveActor
        {
            public class ChildTestActor : ReceiveActor
            {
                public ChildTestActor()
                {
                    Receive<Start>(s => Sender.Tell(new RefWrapper { Ref = new ActorLookup().Parent(Context) }));
                }
            }

            public ParentTestActor()
            {
                Receive<Start>(s => Context.ActorOf<ChildTestActor>().Tell(s, Sender));
            }
        }

        [Fact]
        public void ParentReturnsReferenceToParent()
        {
            var actor = ActorOf<SelfTestActor>();

            actor.Tell(new Start());

            ExpectMsg<RefWrapper>(ar => ar.Ref == actor);
        }

        public sealed class ChildNoChildrenExistTestActor : ReceiveActor
        {
            public ChildNoChildrenExistTestActor()
            {
                Receive<Start>(s => Sender.Tell(new RefWrapper { Ref = new ActorLookup().Child(Context, "TestChildA") }));
            }
        }

        [Fact]
        public void ChildReturnsNobodyWhenNoChildrenExist()
        {
            var actor = ActorOf<ChildNoChildrenExistTestActor>("TestParent");

            actor.Tell(new Start());

            ExpectMsg<RefWrapper>(ar => ar.Ref.IsNobody());
        }

        public sealed class ChildOtherNamesExistTestActor : ReceiveActor
        {
            public class ChildTestActor : ReceiveActor
            {
                public ChildTestActor()
                {
                    Receive<Start>(s => { });
                }
            }

            public ChildOtherNamesExistTestActor()
            {
                Receive<Start>(s =>
                {
                    Context.ActorOf<ChildTestActor>("TestChildA").Tell(s);
                    Context.ActorOf<ChildTestActor>("TestChildC").Tell(s);
                    Sender.Tell(new RefWrapper { Ref = new ActorLookup().Child(Context, "TestChildB") });
                });
            }
        }

        [Fact]
        public void ChildReturnsNobodyWhenOtherNamesExist()
        {
            var actor = ActorOf<ChildOtherNamesExistTestActor>("TestParent");

            actor.Tell(new Start());

            ExpectMsg<RefWrapper>(ar => ar.Ref.IsNobody());
        }

        public sealed class ChildWhenExistsTestActor : ReceiveActor
        {
            public class ChildTestActor : ReceiveActor
            {
                public ChildTestActor()
                {
                    Receive<Start>(s => { });
                }
            }

            public ChildWhenExistsTestActor()
            {
                Receive<Start>(s =>
                {
                    Context.ActorOf<ChildTestActor>("TestChildA").Tell(s);
                    Context.ActorOf<ChildTestActor>("TestChildB").Tell(s);
                    Context.ActorOf<ChildTestActor>("TestChildC").Tell(s);
                    Sender.Tell(new RefWrapper { Ref = new ActorLookup().Child(Context, "TestChildB") });
                });
            }
        }

        [Fact]
        public void ChildReturnsReferenceWhenExists()
        {
            var actor = ActorOf<ChildWhenExistsTestActor>("TestParent");

            actor.Tell(new Start());

            var result = ExpectMsg<RefWrapper>();
            Assert.False(result.Ref.IsNobody());
            Assert.Equal("akka://test/user/TestParent/TestChildB", result.Ref.Path.ToString());
        }

        public sealed class ChildWhenStoppedTestActor : ReceiveActor
        {
            public class ChildTestActor : ReceiveActor
            {
                public ChildTestActor()
                {
                    Receive<Start>(s => { });
                }
            }

            public ChildWhenStoppedTestActor()
            {
                Receive<Start>(s =>
                {
                    var originalSender = Sender;
                    var originalContext = Context;
                    Context.ActorOf<ChildTestActor>("TestChildA").Tell(s);
                    var testChildB = Context.ActorOf<ChildTestActor>("TestChildB");
                    testChildB.Tell(s);
                    testChildB.GracefulStop(TimeSpan.FromSeconds(3)).ContinueWith(t =>
                    {
                        originalContext.ActorOf<ChildTestActor>("TestChildC").Tell(s);
                        originalSender.Tell(new RefWrapper { Ref = new ActorLookup().Child(originalContext, "TestChildB") });
                    });
                });
            }
        }

        [Fact]
        public void ChildReturnsNobodyWhenStopped()
        {
            var actor = ActorOf<ChildWhenStoppedTestActor>("TestParent");

            actor.Tell(new Start());

            ExpectMsg<RefWrapper>(ar => ar.Ref.IsNobody());
        }

        public interface ITestDependency { }
        public sealed class TestDependency : ITestDependency { }

        public sealed class ActorOfWithNameTestActor : ReceiveActor
        {
            public class ChildTestActor : ReceiveActor
            {
                public static readonly SynchronizedCollection<ChildTestActor> Instances = new SynchronizedCollection<ChildTestActor>();

                public readonly ITestDependency TestDependency;
                public readonly IActorContext ContextReference;

                
                public ChildTestActor(ITestDependency testDependency)
                {
                    Instances.Add(this);
                    TestDependency = testDependency;
                    ContextReference = Context;
                    Receive<Start>(s => Sender.Tell(s));
                }
            }

            public ActorOfWithNameTestActor()
            {
                Receive<Start>(s =>
                {
                    var originalSender = Sender;
                    new ActorLookup().ActorOf<ChildTestActor>(Context, "TestChild").Ask<Start>(new Start()).ContinueWith(t => originalSender.Tell(s));
                });
            }
        }

        [Fact]
        public void ActorOfWithNameCreatesOneActor()
        {
            using (var actorSystem = ActorSystem.Create("Sunruse-Influx"))
            {
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterType<ActorOfWithNameTestActor.ChildTestActor>();
                var testDependency = new TestDependency();
                containerBuilder.RegisterInstance(testDependency).As<ITestDependency>();
                new AutoFacDependencyResolver(containerBuilder.Build(), actorSystem);
                var actor = actorSystem.ActorOf<ActorOfWithNameTestActor>("TestParent");
                ActorOfWithNameTestActor.ChildTestActor.Instances.Clear();

                actor.Ask<Start>(new Start());

                ExpectNoMsg();
                Assert.Equal(1, ActorOfWithNameTestActor.ChildTestActor.Instances.Count);
            }
        }

        [Fact]
        public void ActorOfWithNameGivesCorrectName()
        {
            using (var actorSystem = ActorSystem.Create("Sunruse-Influx"))
            {
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterType<ActorOfWithNameTestActor.ChildTestActor>();
                var testDependency = new TestDependency();
                containerBuilder.RegisterInstance(testDependency).As<ITestDependency>();
                new AutoFacDependencyResolver(containerBuilder.Build(), actorSystem);
                var actor = actorSystem.ActorOf<ActorOfWithNameTestActor>("TestParent");
                ActorOfWithNameTestActor.ChildTestActor.Instances.Clear();

                actor.Ask<Start>(new Start());

                ExpectNoMsg();
                Assert.Equal("TestChild", ActorOfWithNameTestActor.ChildTestActor.Instances[0].ContextReference.Self.Path.Name);
            }
        }

        [Fact]
        public void ActorOfWithNameCreatesAsChild()
        {
            using (var actorSystem = ActorSystem.Create("Sunruse-Influx"))
            {
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterType<ActorOfWithNameTestActor.ChildTestActor>();
                var testDependency = new TestDependency();
                containerBuilder.RegisterInstance(testDependency).As<ITestDependency>();
                new AutoFacDependencyResolver(containerBuilder.Build(), actorSystem);
                var actor = actorSystem.ActorOf<ActorOfWithNameTestActor>("TestParent");
                ActorOfWithNameTestActor.ChildTestActor.Instances.Clear();

                actor.Ask<Start>(new Start());

                ExpectNoMsg();
                Assert.Equal(actor, ActorOfWithNameTestActor.ChildTestActor.Instances[0].ContextReference.Parent);
            }
        }

        [Fact]
        public void ActorOfWithNameInjectsDependencies()
        {
            using (var actorSystem = ActorSystem.Create("Sunruse-Influx"))
            {
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterType<ActorOfWithNameTestActor.ChildTestActor>();
                var testDependency = new TestDependency();
                containerBuilder.RegisterInstance(testDependency).As<ITestDependency>();
                new AutoFacDependencyResolver(containerBuilder.Build(), actorSystem);
                var actor = actorSystem.ActorOf<ActorOfWithNameTestActor>("TestParent");
                ActorOfWithNameTestActor.ChildTestActor.Instances.Clear();

                actor.Ask<Start>(new Start());

                ExpectNoMsg();
                Assert.Equal(testDependency, ActorOfWithNameTestActor.ChildTestActor.Instances[0].TestDependency);
            }
        }

        public sealed class ActorOfWithoutNameTestActor : ReceiveActor
        {
            public class ChildTestActor : ReceiveActor
            {
                public static readonly SynchronizedCollection<ChildTestActor> Instances = new SynchronizedCollection<ChildTestActor>();

                public readonly ITestDependency TestDependency;
                public readonly IActorContext ContextReference;


                public ChildTestActor(ITestDependency testDependency)
                {
                    Instances.Add(this);
                    TestDependency = testDependency;
                    ContextReference = Context;
                    Receive<Start>(s => Sender.Tell(s));
                }
            }

            public ActorOfWithoutNameTestActor()
            {
                Receive<Start>(s =>
                {
                    var originalSender = Sender;
                    new ActorLookup().ActorOf<ChildTestActor>(Context).Ask<Start>(new Start()).ContinueWith(t => originalSender.Tell(s));
                });
            }
        }

        [Fact]
        public void ActorOfWithoutNameCreatesOneActor()
        {
            using (var actorSystem = ActorSystem.Create("Sunruse-Influx"))
            {
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterType<ActorOfWithoutNameTestActor.ChildTestActor>();
                var testDependency = new TestDependency();
                containerBuilder.RegisterInstance(testDependency).As<ITestDependency>();
                new AutoFacDependencyResolver(containerBuilder.Build(), actorSystem);
                var actor = actorSystem.ActorOf<ActorOfWithoutNameTestActor>("TestParent");
                ActorOfWithoutNameTestActor.ChildTestActor.Instances.Clear();

                actor.Ask<Start>(new Start());

                ExpectNoMsg();
                Assert.Equal(1, ActorOfWithoutNameTestActor.ChildTestActor.Instances.Count);
            }
        }

        [Fact]
        public void ActorOfWithoutNameGivesCorrectName()
        {
            using (var actorSystem = ActorSystem.Create("Sunruse-Influx"))
            {
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterType<ActorOfWithoutNameTestActor.ChildTestActor>();
                var testDependency = new TestDependency();
                containerBuilder.RegisterInstance(testDependency).As<ITestDependency>();
                new AutoFacDependencyResolver(containerBuilder.Build(), actorSystem);
                var actor = actorSystem.ActorOf<ActorOfWithoutNameTestActor>("TestParent");
                ActorOfWithoutNameTestActor.ChildTestActor.Instances.Clear();

                actor.Ask<Start>(new Start());
                actor.Ask<Start>(new Start());

                ExpectNoMsg();
                Assert.Equal("$a", ActorOfWithoutNameTestActor.ChildTestActor.Instances[0].ContextReference.Self.Path.Name);
                Assert.Equal("$b", ActorOfWithoutNameTestActor.ChildTestActor.Instances[1].ContextReference.Self.Path.Name);
            }
        }

        [Fact]
        public void ActorOfWithoutNameCreatesAsChild()
        {
            using (var actorSystem = ActorSystem.Create("Sunruse-Influx"))
            {
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterType<ActorOfWithoutNameTestActor.ChildTestActor>();
                var testDependency = new TestDependency();
                containerBuilder.RegisterInstance(testDependency).As<ITestDependency>();
                new AutoFacDependencyResolver(containerBuilder.Build(), actorSystem);
                var actor = actorSystem.ActorOf<ActorOfWithoutNameTestActor>("TestParent");
                ActorOfWithoutNameTestActor.ChildTestActor.Instances.Clear();

                actor.Ask<Start>(new Start());

                ExpectNoMsg();
                Assert.Equal(actor, ActorOfWithoutNameTestActor.ChildTestActor.Instances[0].ContextReference.Parent);
            }
        }

        [Fact]
        public void ActorOfWithoutNameInjectsDependencies()
        {
            using (var actorSystem = ActorSystem.Create("Sunruse-Influx"))
            {
                var containerBuilder = new ContainerBuilder();
                containerBuilder.RegisterType<ActorOfWithoutNameTestActor.ChildTestActor>();
                var testDependency = new TestDependency();
                containerBuilder.RegisterInstance(testDependency).As<ITestDependency>();
                new AutoFacDependencyResolver(containerBuilder.Build(), actorSystem);
                var actor = actorSystem.ActorOf<ActorOfWithoutNameTestActor>("TestParent");
                ActorOfWithoutNameTestActor.ChildTestActor.Instances.Clear();

                actor.Ask<Start>(new Start());

                ExpectNoMsg();
                Assert.Equal(testDependency, ActorOfWithoutNameTestActor.ChildTestActor.Instances[0].TestDependency);
            }
        }
    }
}
