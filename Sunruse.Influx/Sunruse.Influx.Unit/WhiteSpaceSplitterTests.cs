using Akka.TestKit.Xunit2;
using Akka.Actor;
using Xunit;
using System.Linq;
using System;

namespace Sunruse.Influx.Unit
{
    public sealed class WhiteSpaceSplitterTests : ConfiguredTestKit
    {
        [Fact]
        public void ReturnsEmptyForEmpty()
        {
            var actor = ActorOf<WhiteSpaceSplitter>();

            actor.Tell(new SplitSourceByWhiteSpace(""));

            var response = ExpectMsgFrom<SourceSplitByWhiteSpace>(actor);
            ExpectNoMsg();
            Assert.Empty(response.Tokens);
        }

        [Fact]
        public void FindsASingleToken()
        {
            var actor = ActorOf<WhiteSpaceSplitter>();

            actor.Tell(new SplitSourceByWhiteSpace("testToken2378$"));

            var response = ExpectMsgFrom<SourceSplitByWhiteSpace>(actor);
            ExpectNoMsg();
            var responseTokens = response.Tokens.ToList();
            Assert.Equal(1, responseTokens.Count);
            Assert.Equal("testToken2378$", responseTokens[0].Text);
            Assert.Equal(0, responseTokens[0].Origin.StartIndex);
            Assert.Equal(13, responseTokens[0].Origin.EndIndex);
        }

        [Fact]
        public void FindsASingleTokenPrecededByWhiteSpace()
        {
            var actor = ActorOf<WhiteSpaceSplitter>();

            actor.Tell(new SplitSourceByWhiteSpace("   \n   \r \t  testToken2378$"));

            var response = ExpectMsgFrom<SourceSplitByWhiteSpace>(actor);
            ExpectNoMsg();
            var responseTokens = response.Tokens.ToList();
            Assert.Equal(1, responseTokens.Count);
            Assert.Equal("testToken2378$", responseTokens[0].Text);
            Assert.Equal(12, responseTokens[0].Origin.StartIndex);
            Assert.Equal(25, responseTokens[0].Origin.EndIndex);
        }

        [Fact]
        public void FindsASingleTokenFollowedByWhiteSpace()
        {
            var actor = ActorOf<WhiteSpaceSplitter>();

            actor.Tell(new SplitSourceByWhiteSpace("testToken2378$   \n   \r \t  "));

            var response = ExpectMsgFrom<SourceSplitByWhiteSpace>(actor);
            ExpectNoMsg();
            var responseTokens = response.Tokens.ToList();
            Assert.Equal(1, responseTokens.Count);
            Assert.Equal("testToken2378$", responseTokens[0].Text);
            Assert.Equal(0, responseTokens[0].Origin.StartIndex);
            Assert.Equal(13, responseTokens[0].Origin.EndIndex);
        }

        [Fact]
        public void FindsMultipleTokens()
        {
            var actor = ActorOf<WhiteSpaceSplitter>();

            actor.Tell(new SplitSourceByWhiteSpace("testToken2378$  \n \t \r SOMEmoretokens 2furtherTOKEN   \n \r \t followingNewlinesToken \n \r \t WithMORE!after \n \t \r andMoreHere"));

            var response = ExpectMsgFrom<SourceSplitByWhiteSpace>(actor);
            ExpectNoMsg();
            var responseTokens = response.Tokens.ToList();
            Assert.Equal(6, responseTokens.Count);
            Assert.Equal("testToken2378$", responseTokens[0].Text);
            Assert.Equal(0, responseTokens[0].Origin.StartIndex);
            Assert.Equal(13, responseTokens[0].Origin.EndIndex);
            Assert.Equal("SOMEmoretokens", responseTokens[1].Text);
            Assert.Equal(22, responseTokens[1].Origin.StartIndex);
            Assert.Equal(35, responseTokens[1].Origin.EndIndex);
            Assert.Equal("2furtherTOKEN", responseTokens[2].Text);
            Assert.Equal(37, responseTokens[2].Origin.StartIndex);
            Assert.Equal(49, responseTokens[2].Origin.EndIndex);
            Assert.Equal("followingNewlinesToken", responseTokens[3].Text);
            Assert.Equal(59, responseTokens[3].Origin.StartIndex);
            Assert.Equal(80, responseTokens[3].Origin.EndIndex);
            Assert.Equal("WithMORE!after", responseTokens[4].Text);
            Assert.Equal(88, responseTokens[4].Origin.StartIndex);
            Assert.Equal(101, responseTokens[4].Origin.EndIndex);
            Assert.Equal("andMoreHere", responseTokens[5].Text);
            Assert.Equal(109, responseTokens[5].Origin.StartIndex);
            Assert.Equal(119, responseTokens[5].Origin.EndIndex);
        }

        [Fact]
        public void FindsMultipleTokensWithinWhiteSpace()
        {
            var actor = ActorOf<WhiteSpaceSplitter>();

            actor.Tell(new SplitSourceByWhiteSpace(" \n \r \t testToken2378$  \n \t \r SOMEmoretokens 2furtherTOKEN   \n \r \t followingNewlinesToken \n \r \t WithMORE!after \n \t \r andMoreHere \r \n \n \t "));

            var response = ExpectMsgFrom<SourceSplitByWhiteSpace>(actor);
            ExpectNoMsg();
            var responseTokens = response.Tokens.ToList();
            Assert.Equal(6, responseTokens.Count);
            Assert.Equal("testToken2378$", responseTokens[0].Text);
            Assert.Equal(7, responseTokens[0].Origin.StartIndex);
            Assert.Equal(20, responseTokens[0].Origin.EndIndex);
            Assert.Equal("SOMEmoretokens", responseTokens[1].Text);
            Assert.Equal(29, responseTokens[1].Origin.StartIndex);
            Assert.Equal(42, responseTokens[1].Origin.EndIndex);
            Assert.Equal("2furtherTOKEN", responseTokens[2].Text);
            Assert.Equal(44, responseTokens[2].Origin.StartIndex);
            Assert.Equal(56, responseTokens[2].Origin.EndIndex);
            Assert.Equal("followingNewlinesToken", responseTokens[3].Text);
            Assert.Equal(66, responseTokens[3].Origin.StartIndex);
            Assert.Equal(87, responseTokens[3].Origin.EndIndex);
            Assert.Equal("WithMORE!after", responseTokens[4].Text);
            Assert.Equal(95, responseTokens[4].Origin.StartIndex);
            Assert.Equal(108, responseTokens[4].Origin.EndIndex);
            Assert.Equal("andMoreHere", responseTokens[5].Text);
            Assert.Equal(116, responseTokens[5].Origin.StartIndex);
            Assert.Equal(126, responseTokens[5].Origin.EndIndex);
        }
    }
}