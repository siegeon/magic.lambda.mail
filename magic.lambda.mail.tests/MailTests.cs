/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System.Threading.Tasks;
using Xunit;

namespace magic.lambda.mail.tests
{
    public class MailTests
    {
        [Fact]
        public async Task ConnectTestExplicitServerAsync()
        {
            var connectInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.smtp.send
   server
      host:foo.com
      port:123
      secure:true
      username:xxx
      password:yyy
   message
      to
         John Doe:john@doe.com
      from
         Jane Doe:jane@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content",
                (msg) => { },
                (host, port, useSsl) =>
                {
                    Assert.Equal("foo.com", host);
                    Assert.Equal(123, port);
                    Assert.True(useSsl);
                    connectInvoked = true;
                });
            Assert.True(connectInvoked);
        }

        [Fact]
        public async Task ConnectTestConfigurationServerAsync_01()
        {
            var connectInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.smtp.send
   server
      port:123
      secure:true
      username:xxx
      password:yyy
   message
      to
         John Doe:john@doe.com
      from
         Jane Doe:jane@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content",
                (msg) => { },
                (host, port, useSsl) =>
                {
                    Assert.Equal("foo2.com", host);
                    Assert.Equal(123, port);
                    Assert.True(useSsl);
                    connectInvoked = true;
                });
            Assert.True(connectInvoked);
        }

        [Fact]
        public async Task ConnectTestConfigurationServerAsync_02()
        {
            var connectInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.smtp.send
   server
      username:xxx
      password:yyy
   message
      to
         John Doe:john@doe.com
      from
         Jane Doe:jane@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content",
                (msg) => { },
                (host, port, useSsl) =>
                {
                    Assert.Equal("foo2.com", host);
                    Assert.Equal(321, port);
                    Assert.False(useSsl);
                    connectInvoked = true;
                });
            Assert.True(connectInvoked);
        }

        [Fact]
        public async Task ConnectTestConnectConnectAsync_01()
        {
            var authenticateInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.smtp.send
   server
      username:xxx
      password:yyy
   message
      to
         John Doe:john@doe.com
      from
         Jane Doe:jane@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content",
                (msg) => { },
                null,
                (username, password) =>
                {
                    Assert.Equal("xxx", username);
                    Assert.Equal("yyy", password);
                    authenticateInvoked = true;
                });
            Assert.True(authenticateInvoked);
        }

        [Fact]
        public async Task ConnectTestConnectConnectAsync_02()
        {
            var authenticateInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.smtp.send
   server
   message
      to
         John Doe:john@doe.com
      from
         Jane Doe:jane@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content",
                (msg) => { },
                null,
                (username, password) =>
                {
                    Assert.Equal("xxx2", username);
                    Assert.Equal("yyy2", password);
                    authenticateInvoked = true;
                });
            Assert.True(authenticateInvoked);
        }

        [Fact]
        public async Task ConnectTestConnectConnectAsync_03()
        {
            var authenticateInvoked = false;
            var connectInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.smtp.send
   message
      to
         John Doe:john@doe.com
      from
         Jane Doe:jane@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content",
                (msg) => { },
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
                });
            Assert.True(authenticateInvoked);
            Assert.True(connectInvoked);
        }
    }
}
