/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2021, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using MimeKit;
using MailKit;
using mk = MailKit.Net.Smtp;
using magic.lambda.mime.contracts;

namespace magic.lambda.mime.services
{
    /// <inheritdoc/>
    public sealed class SmtpClient : MailClient, ISmtpClient, IDisposable
    {
        readonly Lazy<mk.SmtpClient> _client = new Lazy<mk.SmtpClient>(() => new mk.SmtpClient());

        /// <inheritdic/>
        public override IMailService Client => _client.Value;

        /// <inheritdoc/>
        public void Send(MimeMessage message)
        {
            _client.Value.Send(message);
        }

        /// <inheritdoc/>
        public async Task SendAsync(MimeMessage message)
        {
            await _client.Value.SendAsync(message);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_client.IsValueCreated)
                Client.Dispose();
        }
    }
}
