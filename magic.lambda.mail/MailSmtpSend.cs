/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.mime.helpers;

namespace magic.lambda.mime
{
    /// <summary>
    /// Sends a single email message through an SMTP server.
    /// </summary>
    [Slot(Name = "wait.mail.smtp.send")]
    public class MailSmtpSend : ISlotAsync
    {
        readonly IConfiguration _configuration;

        public MailSmtpSend(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Retrieving server connection settings.
            var settings = new ConnectionSettings(_configuration, input, "smtp");

            // Retrieving message we should actually send.
            var messageNode = input.Children.FirstOrDefault(x => x.Name == "message") ??
                throw new ArgumentNullException("No [message] provided to [wait.mail.smtp.send]");

            // Creating MimeMessage.
            var message = new MimeMessage();
            signaler.Signal(".mime.create", messageNode);
            message.Body = messageNode.Value as MimeEntity;

            // Decorating MimeMessage with subject.
            var subject = input.Children.FirstOrDefault(x => x.Name == "subject")?.GetEx<string>();
            message.Subject = subject;

            // Decorating MimeMessage with from/sender.
            var from = input.Children.FirstOrDefault(x => x.Name == "from")?.GetEx<string>() ??
                throw new ArgumentNullException("No [from] sender given in your email");
            var fromName = input.Children.FirstOrDefault(x => x.Name == "from-name")?.GetEx<string>();
            message.From.Add(new MailboxAddress(fromName, from));

            // Decorating MimeMessage with to.
            var to = input.Children.FirstOrDefault(x => x.Name == "to")?.GetEx<string>() ??
                throw new ArgumentNullException("No [to] recipient given in your email");
            var toName = input.Children.FirstOrDefault(x => x.Name == "to-name")?.GetEx<string>();
            message.To.Add(new MailboxAddress(toName, to));

            // Creating client, and sending message.
            using (var client = new SmtpClient())
            {
                // Connecting and authenticating (unless username is null)
                await client.ConnectAsync(settings.Server, settings.Port, settings.Secure);
                if (settings.Username != null)
                    await client.AuthenticateAsync(settings.Username, settings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }
}
