/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using MimeKit;
using MailKit.Net.Smtp;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;

namespace magic.lambda.mime
{
    /// <summary>
    /// Sends a single email message through an SMTP server.
    /// </summary>
    [Slot(Name = "wait.mail.smtp.send")]
    public class MailSmtpSend : ISlotAsync
    {
        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Retrieving connection arguments.
            var server = input.Children.SingleOrDefault(x => x.Name == "server")?.GetEx<string>() ??
                throw new ArgumentNullException("No [server] provided to [wait.mail.smtp.send]");
            var port = input.Children.SingleOrDefault(x => x.Name == "port")?.GetEx<int>() ??
                throw new ArgumentNullException("No [port] provided to [wait.mail.smtp.send]");
            var ssl = input.Children.SingleOrDefault(x => x.Name == "secure")?.GetEx<bool>() ?? false;
            var username = input.Children.SingleOrDefault(x => x.Name == "username")?.GetEx<string>();
            var password = input.Children.SingleOrDefault(x => x.Name == "password")?.GetEx<string>();

            // Retrieving message we should actually send.
            var messageNode = input.Children.FirstOrDefault(x => x.Name == "entity") ??
                throw new ArgumentNullException("No [message] provided to [wait.mail.smtp.send]");

            // Creating MimeMessage.
            var message = new MimeMessage();
            signaler.Signal(".mime.create", messageNode);
            message.Body = messageNode.Value as MimeEntity;

            // Decorating MimeMessage with subject, from, to, etc.
            var subject = input.Children.FirstOrDefault(x => x.Name == "subject")?.GetEx<string>();
            message.Subject = subject;

            var from = input.Children.FirstOrDefault(x => x.Name == "from")?.GetEx<string>() ??
                throw new ArgumentNullException("No [from] sender given in your email");
            var fromName = input.Children.FirstOrDefault(x => x.Name == "from-name")?.GetEx<string>();
            message.From.Add(new MailboxAddress(fromName, from));

            var to = input.Children.FirstOrDefault(x => x.Name == "to")?.GetEx<string>() ??
                throw new ArgumentNullException("No [to] recipient given in your email");
            var toName = input.Children.FirstOrDefault(x => x.Name == "to-name")?.GetEx<string>();
            message.To.Add(new MailboxAddress(fromName, from));

            // Creating client, and sending [message].
            using (var client = new SmtpClient())
            {
                // Connecting and authenticating (unless username is null)
                await client.ConnectAsync(server, port, ssl);
                if (username != null)
                    await client.AuthenticateAsync(username, password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
