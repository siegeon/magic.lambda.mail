/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Threading.Tasks;
using Xunit;

namespace magic.lambda.mail.tests
{
    public class PopTests
    {
        [Fact]
        public async Task ConnectWithServer()
        {
            var connectInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.pop3.fetch
   server
      host:foo.com
      port:123
      secure:true
      username:xxx
      password:yyy
   .lambda",
                null,
                new helpers.MockPop3Client(
                    (index) =>
                    {
                        return null;
                    },
                    () => 0,
                    (host, port, useSsl) =>
                    {
                        Assert.Equal("foo.com", host);
                        Assert.Equal(123, port);
                        Assert.True(useSsl);
                        connectInvoked = true;
                    }));
            Assert.True(connectInvoked);
        }

        [Fact]
        public async Task ConnectWithConfig_01()
        {
            var connectInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.pop3.fetch
   server
      port:123
      secure:true
      username:xxx
      password:yyy
   .lambda",
                null,
                new helpers.MockPop3Client(
                    (index) =>
                    {
                        return null;
                    },
                    () => 0,
                    (host, port, useSsl) =>
                    {
                        Assert.Equal("foo2.com", host);
                        Assert.Equal(123, port);
                        Assert.True(useSsl);
                        connectInvoked = true;
                    }));
            Assert.True(connectInvoked);
        }

        [Fact]
        public async Task ConnectWithConfig_02()
        {
            var connectInvoked = false;
            var authenticateInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.pop3.fetch
   server
      username:xxx
      password:yyy
   .lambda",
                null,
                new helpers.MockPop3Client(
                    (index) =>
                    {
                        return null;
                    },
                    () => 0,
                    (host, port, useSsl) =>
                    {
                        Assert.Equal("foo2.com", host);
                        Assert.Equal(321, port);
                        Assert.False(useSsl);
                        connectInvoked = true;
                    },
                    (username, password) =>
                    {
                        Assert.Equal("xxx", username);
                        Assert.Equal("yyy", password);
                        authenticateInvoked = true;
                    }));
            Assert.True(connectInvoked);
            Assert.True(authenticateInvoked);
        }

        [Fact]
        public async Task ConcectWithConfig_03()
        {
            var authenticateInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.pop3.fetch
   server
   .lambda",
                null,
                new helpers.MockPop3Client(
                    (index) =>
                    {
                        return null;
                    },
                    () => 0,
                    null,
                    (username, password) =>
                    {
                        Assert.Equal("xxx2", username);
                        Assert.Equal("yyy2", password);
                        authenticateInvoked = true;
                    }));
            Assert.True(authenticateInvoked);
        }

        [Fact]
        public async Task ConnectWithConfig_04()
        {
            var authenticateInvoked = false;
            var connectInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.pop3.fetch
   .lambda",
                null,
                new helpers.MockPop3Client(
                    (index) =>
                    {
                        return null;
                    },
                    () => 0,
                    (host, port, useSsl) =>
                    {
                        Assert.Equal("foo2.com", host);
                        Assert.Equal(321, port);
                        Assert.False(useSsl);
                        connectInvoked = true;
                    },
                    (username, password) =>
                    {
                        Assert.Equal("xxx2", username);
                        Assert.Equal("yyy2", password);
                        authenticateInvoked = true;
                    }));
            Assert.True(authenticateInvoked);
            Assert.True(connectInvoked);
        }
    }
}
