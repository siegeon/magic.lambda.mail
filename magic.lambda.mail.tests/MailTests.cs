/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using MimeKit;

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

        [Fact]
        public async Task SendAsync_01()
        {
            var sendInvoked = false;
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
                (msg) =>
                {
                    Assert.NotNull(msg);
                    Assert.NotEqual(typeof(Multipart), msg.Body.GetType());
                    Assert.Equal("text", msg.Body.ContentType.MediaType);
                    Assert.Equal("plain", msg.Body.ContentType.MediaSubtype);
                    Assert.Equal("Subject line", msg.Subject);
                    Assert.Single(msg.To);
                    Assert.Equal("John Doe", msg.To.First().Name);
                    Assert.Equal("john@doe.com", (msg.To.First() as MailboxAddress).Address);
                    Assert.Single(msg.From);
                    Assert.Equal("Jane Doe", msg.From.First().Name);
                    Assert.Equal("jane@doe.com", (msg.From.First() as MailboxAddress).Address);
                    Assert.Empty(msg.Cc);
                    Assert.Empty(msg.Bcc);
                    Assert.Equal(@"Content-Type: text/plain

Body content", msg.Body.ToString());
                    sendInvoked = true;
                },
                null,
                null);
            Assert.True(sendInvoked);
        }

        [Fact]
        public async Task SendAsync_02()
        {
            var sendInvoked = false;
            var lambda = await Common.EvaluateAsync(@"
wait.mail.smtp.send
   message
      to
         John Doe:john@doe.com
      from
         Jane Doe:jane@doe.com
      cc
         Peter Doe:peter@doe.com
      bcc
         Peter Doe 1:peter1@doe.com
         Peter Doe 2:peter2@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content",
                (msg) =>
                {
                    Assert.NotNull(msg);
                    Assert.NotEqual(typeof(Multipart), msg.Body.GetType());
                    Assert.Equal("text", msg.Body.ContentType.MediaType);
                    Assert.Equal("plain", msg.Body.ContentType.MediaSubtype);
                    Assert.Equal("Subject line", msg.Subject);
                    Assert.Single(msg.To);
                    Assert.Equal("John Doe", msg.To.First().Name);
                    Assert.Equal("john@doe.com", (msg.To.First() as MailboxAddress).Address);
                    Assert.Single(msg.From);
                    Assert.Equal("Jane Doe", msg.From.First().Name);
                    Assert.Equal("jane@doe.com", (msg.From.First() as MailboxAddress).Address);
                    Assert.Single(msg.Cc);
                    Assert.Equal("Peter Doe", msg.Cc.First().Name);
                    Assert.Equal("peter@doe.com", (msg.Cc.First() as MailboxAddress).Address);
                    Assert.Equal(2, msg.Bcc.Count);
                    Assert.Equal("Peter Doe 1", msg.Bcc.First().Name);
                    Assert.Equal("peter1@doe.com", (msg.Bcc.First() as MailboxAddress).Address);
                    Assert.Equal("Peter Doe 2", msg.Bcc.Skip(1).First().Name);
                    Assert.Equal("peter2@doe.com", (msg.Bcc.Skip(1).First() as MailboxAddress).Address);
                    sendInvoked = true;
                },
                null,
                null);
            Assert.True(sendInvoked);
        }

        [Fact]
        public async Task SendAsync_02_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await Common.EvaluateAsync(@"
wait.mail.smtp.send
   message
      from
         Jane Doe:jane@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content",
                    (msg) => { },
                    null,
                    null);
            });
        }

        [Fact]
        public async Task SendAsync_03_Throws()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await Common.EvaluateAsync(@"
wait.mail.smtp.send
   message
      to
         Jane Doe:jane@doe.com
      subject:Subject line
      entity:text/plain
         content:Body content",
                    (msg) => { },
                    null,
                    null);
            });
        }
    }
}
