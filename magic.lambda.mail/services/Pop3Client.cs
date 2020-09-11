/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Threading.Tasks;
using MimeKit;
using MailKit;
using mk = MailKit.Net.Pop3;
using magic.lambda.mime.contracts;

namespace magic.lambda.mime.services
{
    /// <inheritdoc/>
    public sealed class Pop3Client : MailClient, IPop3Client
    {
        readonly Lazy<mk.Pop3Client> _client = new Lazy<mk.Pop3Client>(() => new mk.Pop3Client());

        /// <inheritdic/>
        public override IMailService Client => _client.Value;

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

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_client.IsValueCreated)
                Client.Dispose();
        }
    }
}
