/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using MimeKit;
using mk = MailKit.Net.Smtp;
using contract = magic.lambda.mime.contracts;

namespace magic.lambda.mime.services
{
    /// <inheritdoc/>
    public sealed class SmtpClient : contract.ISmtpClient
    {
        readonly Lazy<mk.SmtpClient> _client = new Lazy<mk.SmtpClient>(() => new mk.SmtpClient());

        /// <inheritdoc/>
        public void Authenticate(string username, string password)
        {
            _client.Value.Authenticate(username, password);
        }

        /// <inheritdoc/>
        public async Task AuthenticateAsync(string username, string password)
        {
            await _client.Value.AuthenticateAsync(username, password);
        }

        /// <inheritdoc/>
        public void Connect(string host, int port, bool useSsl)
        {
            _client.Value.Connect(host, port, useSsl);
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(string host, int port, bool useSsl)
        {
            await _client.Value.ConnectAsync(host, port, useSsl);
        }

        /// <inheritdoc/>
        public void Disconnect(bool quit)
        {
            _client.Value.Disconnect(quit);
        }

        /// <inheritdoc/>
        public async Task DisconnectAsync(bool quit)
        {
            await _client.Value.DisconnectAsync(quit);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _client.Value?.Dispose();
        }

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
    }
}
