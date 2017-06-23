using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sunruse.Influx
{
    /// <summary>Splits given source code by white-space into tokens.</summary>
    public sealed class WhiteSpaceSplitter : ReceiveActor
    {
        /// <inheritdoc />
        public WhiteSpaceSplitter()
        {
            Receive<SplitSourceByWhiteSpace>(ssbw =>
            {
                var tokens = new List<UntypedToken>();
                string token = null;
                for (var i = 0; i < ssbw.Source.Length; i++)
                {
                    var character = ssbw.Source[i];
                    if (Char.IsWhiteSpace(character))
                    {
                        if (token != null)
                        {
                            tokens.Add(new UntypedToken(token, new Origin(i - token.Length, i - 1)));
                            token = null;
                        }
                    }
                    else token = (token ?? "") + character;
                }
                if (token != null) tokens.Add(new UntypedToken(token, new Origin(ssbw.Source.Length - token.Length, ssbw.Source.Length - 1)));
                    Sender.Tell(new SourceSplitByWhiteSpace(tokens));
            });
        }
    }

    /// <summary>A request to <see cref="WhiteSpaceSplitter"/> to split source code by white-space.</summary>
    public sealed class SplitSourceByWhiteSpace
    {
        /// <summary>The source code to split by white-space.</summary>
        public readonly string Source;

        /// <inheritdoc />
        /// <param name="source"><see cref="Source"/>.</param>
        public SplitSourceByWhiteSpace(string source)
        {
            Source = source;
        }
    }

    /// <summary>Returned from <see cref="WhiteSpaceSplitter"/> in response to <see cref="SplitSourceByWhiteSpace"/>.</summary>
    public sealed class SourceSplitByWhiteSpace
    {
        /// <summary>The <see cref="UntypedToken"/>s found by splitting <see cref="SplitSourceByWhiteSpace.Source"/>.</summary>
        public readonly IEnumerable<UntypedToken> Tokens;

        /// <inheritdoc />
        /// <param name="tokens"><see cref="Tokens"/>.</param>
        public SourceSplitByWhiteSpace(IEnumerable<UntypedToken> tokens)
        {
            Tokens = tokens;
        }
    }

    /// <summary>Describes where in the source code something came from.</summary>
    public sealed class Origin
    {
        /// <summary>The inclusive earliest index <see langword="this"/> was defined in.</summary>
        public readonly int StartIndex;

        /// <summary>The inclusive latest index <see langword="this"/> was defined in.</summary>
        public readonly int EndIndex;

        /// <inheritdoc />
        /// <param name="startIndex"><see cref="StartIndex"/>.</param>
        /// <param name="endIndex"><see cref="EndIndex"/>.</param>
        public Origin(int startIndex, int endIndex)
        {
            StartIndex = startIndex;
            EndIndex = endIndex;
        }
    }

    /// <summary>Represents a token which has been split from source code by <see cref="WhiteSpaceSplitter"/>.</summary>
    public sealed class UntypedToken
    {
        /// <summary>The <see cref="string"/> split from source code.</summary>
        public readonly string Text;

        /// <summary>The <see cref="Origin"/> of <see cref="Text"/>.</summary>
        public readonly Origin Origin;

        /// <inheritdoc />
        /// <param name="text"><see cref="Text"/>.</param>
        /// <param name="origin"><see cref="Origin"/>.</param>
        public UntypedToken(string text, Origin origin)
        {
            Text = text;
            Origin = origin;
        }
    }
}
