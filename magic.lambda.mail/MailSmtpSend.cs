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
using System.Collections.Generic;

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
            var settings = new ConnectionSettings(
                _configuration,
                input.Children.FirstOrDefault(x => x.Name == "server"),
                "smtp");

            // Creating SMTP client.
            using (var client = new SmtpClient())
            {
                // Connecting and authenticating (unless username is null).
                await client.ConnectAsync(settings.Server, settings.Port, settings.Secure);
                try
                {
                    // Checking if we're supposed to authenticate
                    if (settings.Username != null || settings.Password != null)
                        await client.AuthenticateAsync(settings.Username, settings.Password);

                    // Iterating through each message and sending.
                    foreach (var idxMsgNode in input.Children.Where(x => x.Name == "message"))
                    {
                        // Creating MimeMessage.
                        var message = new MimeMessage();
                        var clone = idxMsgNode.Clone();
                        signaler.Signal(".mime.create", clone);
                        message.Body = clone.Value as MimeEntity;

                        // Decorating MimeMessage with subject.
                        var subject = idxMsgNode.Children.FirstOrDefault(x => x.Name == "subject")?.GetEx<string>();
                        message.Subject = subject;

                        // Decorating MimeMessage with from, to, cc, and bcc.
                        message.From.AddRange(GetAddresses(idxMsgNode.Children.FirstOrDefault(x => x.Name == "from")));
                        message.To.AddRange(GetAddresses(idxMsgNode.Children.FirstOrDefault(x => x.Name == "to")));
                        message.Cc.AddRange(GetAddresses(idxMsgNode.Children.FirstOrDefault(x => x.Name == "cc")));
                        message.Bcc.AddRange(GetAddresses(idxMsgNode.Children.FirstOrDefault(x => x.Name == "bcc")));

                        // Sending message over existing SMTP connection.
                        await client.SendAsync(message);
                    }
                }
                finally
                {
                    // Disconnecting and sending QUIT signal to SMTP server.
                    await client.DisconnectAsync(true);
                }
            }
        }

        #region [ -- Private helpers -- ]

        /*
         * Returns a bunch of email addresses by iterating the children of the specified node,
         * and transforming each into a valid MailboxAddress.
         */
        IEnumerable<MailboxAddress> GetAddresses(Node iterator)
        {
            if (iterator == null)
                yield break;
            foreach (var idx in iterator.Children)
            {
                yield return new MailboxAddress(idx.Name, idx.GetEx<string>());
            }
        }

        #endregion
    }
}
