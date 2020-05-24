/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MimeKit;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

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

        [Slot(Name = "RetrieveOneMessageSlot")]
        class RetrieveOneMessageSlot : ISlot
        {
            public static int _invocationCount = 0;
            public void Signal(ISignaler signaler, Node input)
            {
                _invocationCount += 1;
                var entity = input.Children.SingleOrDefault(x => x.Name == "entity");
                Assert.NotNull(entity);
                Assert.Equal("text/plain", entity.GetEx<string>());
                Assert.Single(entity.Children);
                Assert.Equal("content", entity.Children.First().Name);
                Assert.Equal("Body of message", entity.Children.First().GetEx<string>());
                Assert.Single(entity.Children);
            }
        }

        [Fact]
        public async Task RetrieveOneMessage()
        {
            var authenticateInvoked = false;
            var connectInvoked = false;
            var retrieveInvoked = 0;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.pop3.fetch
   .lambda
      add:x:+
         get-nodes:x:@.message/*
      RetrieveOneMessageSlot",
                null,
                new helpers.MockPop3Client(
                    (index) =>
                    {
                        Assert.Equal(0, index);
                        retrieveInvoked += 1;
                        var message = new MimeMessage();
                        message.From.Add(new MailboxAddress("Joey", "joey@friends.com"));
                        message.To.Add(new MailboxAddress("Alice", "alice@wonderland.com"));
                        message.Subject = "How you doin?";

                        message.Body = new TextPart("plain")
                        {
                            Text = @"Body of message"
                        };
                        return message;
                    },
                    () =>
                    {
                        return 1;
                    },
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
            Assert.Equal(1, retrieveInvoked);
            Assert.Equal(1, RetrieveOneMessageSlot._invocationCount);
        }
    }
}
