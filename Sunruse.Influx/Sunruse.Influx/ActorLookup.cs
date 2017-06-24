using Akka.Actor;
using Akka.DI.Core;
using System;

namespace Sunruse.Influx
{
    /// <summary>Proxies the varioos ways of getting Actors from within Actors; used to mock them out for unit tests.</summary>
    public interface IActorLookup
    {
        /// <summary>Gets the <see cref="IActorContext.Parent"/> of an <see cref="IActorContext"/>.</summary>
        /// <param name="context">The current <see cref="IActorContext"/>.</param>
        /// <returns>The <see cref="IActorContext.Parent"/> of <paramref name="context"/>.</returns>
        IActorRef Parent(IActorContext context);

        /// <summary>Gets the <see cref="IActorContext.Self"/> of an <see cref="IActorContext"/>.</summary>
        /// <param name="context">The current <see cref="IActorContext"/>.</param>
        /// <returns>The <see cref="IActorContext.Self"/> of <paramref name="context"/>.</returns>
        IActorRef Self(IActorContext context);

        /// <summary>Gets a child of an <see cref="IActorContext"/> by its <see cref="ActorPath.Name"/>, or <see cref="Nobody"/> if it cannot be found.</summary>
        /// <param name="context">The current <see cref="IActorContext"/>.</param>
        /// <param name="name">The <see cref="ActorPath.Name"/> of the child to retrieve.</param>
        /// <returns>If a child Actor with a <see cref="ActorPath.Name"/> matching <paramref name="name"/> exists, an <see cref="IActorRef"/> to that Actor, else, <see cref="Nobody"/>.</returns>
        IActorRef Child(IActorContext context, string name);

        /// <summary>Creates a child Actor using DI.</summary>
        /// <typeparam name="TActor">The <see cref="Type"/> of the Actor to create.</typeparam>
        /// <param name="context">The current <see cref="IActorContext"/>.</param>
        /// <param name="name">The <see cref="ActorPath.Name"/> of the Actor to create.</param>
        /// <returns>An <see cref="IActorRef"/> to the created Actor.</returns>
        IActorRef ActorOf<TActor>(IActorContext context, string name = null) where TActor : ActorBase;
    }

    /// <inheritdoc />
    public sealed class ActorLookup : IActorLookup
    {
        /// <inheritdoc />
        public IActorRef Parent(IActorContext context)
        {
            return context.Parent;
        }

        /// <inheritdoc />
        public IActorRef Self(IActorContext context)
        {
            return context.Self;
        }

        /// <inheritdoc />
        public IActorRef Child(IActorContext context, string name)
        {
            return context.Child(name);
        }

        /// <inheritdoc />
        public IActorRef ActorOf<TActor>(IActorContext context, string name = null) where TActor : ActorBase
        {
            return context.ActorOf(context.DI().Props<TActor>(), name);
        }
    }
}
