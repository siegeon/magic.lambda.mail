﻿/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Threading.Tasks;
using MailKit;
using magic.lambda.mime.contracts;

namespace magic.lambda.mime.services
{
    /// <inheritdoc/>
    public abstract class MailClient : IMailClient
    {
        /// <inheritdoc/>
        public void Authenticate(string username, string password)
        {
            Client.Authenticate(username, password);
        }

        /// <inheritdoc/>
        public async Task AuthenticateAsync(string username, string password)
        {
            await Client.AuthenticateAsync(username, password);
        }

        /// <inheritdoc/>
        public void Connect(string host, int port, bool useSsl)
        {
            Client.Connect(host, port, useSsl);
        }

        /// <inheritdoc/>
        public async Task ConnectAsync(string host, int port, bool useSsl)
        {
            await Client.ConnectAsync(host, port, useSsl);
        }

        /// <inheritdoc/>
        public void Disconnect(bool quit)
        {
            Client.Disconnect(quit);
        }

        /// <inheritdoc/>
        public async Task DisconnectAsync(bool quit)
        {
            await Client.DisconnectAsync(quit);
        }

        /// <summary>
        /// Returns the underlaying mail client to caller.
        /// </summary>
        /// <value>Actual mail client.</value>
        public abstract IMailService Client { get; }
    }
}
