﻿/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System.Threading.Tasks;
using MimeKit;

namespace magic.lambda.mime.contracts
{
    /// <summary>
    /// Abstract SMTP interface dependency injected into SMTP sender class.
    /// </summary>
    public interface ISmtpClient : IMailClient
    {
        /// <summary>
        /// Sends a MIME message over an already open SMTP connection.
        /// </summary>
        /// <param name="message">Message to send</param>
        void Send(MimeMessage message);

        /// <summary>
        /// Sends a MIME message over an already open SMTP connection.
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>Awaitable task</returns>
        Task SendAsync(MimeMessage message);
    }
}
