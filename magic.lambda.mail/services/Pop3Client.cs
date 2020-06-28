/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using MimeKit;
using mk = MailKit.Net.Pop3;
using contract = magic.lambda.mime.contracts;

namespace magic.lambda.mime.services
{
    /// <inheritdoc/>
    public sealed class Pop3Client : contract.IPop3Client
    {
        readonly Lazy<mk.Pop3Client> _client = new Lazy<mk.Pop3Client>(() => new mk.Pop3Client());

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
        public MimeMessage GetMessage(int index)
        {
            return _client.Value.GetMessage(index);
        }

        /// <inheritdoc/>
        public async Task<MimeMessage> GetMessageAsync(int index)
        {
            return await _client.Value.GetMessageAsync(index);
        }

        /// <inheritdoc/>
        public int GetMessageCount()
        {
            return _client.Value.GetMessageCount();
        }

        /// <inheritdoc/>
        public async Task<int> GetMessageCountAsync()
        {
            return await _client.Value.GetMessageCountAsync();
        }
    }
}
