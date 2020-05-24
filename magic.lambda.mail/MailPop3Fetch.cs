/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Pop3;
using magic.node;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.mail.helpers;
using contracts = magic.lambda.mime.contracts;

namespace magic.lambda.mail
{
    /// <summary>
    /// Fetches all new messages from the specified POP3 account.
    /// </summary>
    [Slot(Name = "mail.pop3.fetch")]
    [Slot(Name = "wait.mail.pop3.fetch")]
    public class MailPop3Fetch : ISlotAsync, ISlot
    {
        readonly IConfiguration _configuration;
        readonly contracts.IPop3Client _client;

        public MailPop3Fetch(IConfiguration configuration, contracts.IPop3Client client)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            // Retrieving server connection settings.
            var settings = new ConnectionSettings(_configuration, input, "pop3");

            // Maximum number of emails to fetch.
            var max = input.Children.SingleOrDefault(x => x.Name == "max")?.GetEx<int>() ?? 50;
            var raw = input.Children.SingleOrDefault(x => x.Name == "raw")?.GetEx<bool>() ?? false;

            // Retrieving lambda callback for what to do for each message.
            var lambda = input.Children.FirstOrDefault(x => x.Name == ".lambda") ??
                throw new ArgumentNullException("No [.lambda] provided to [wait.mail.pop3.fetch]");

            // Connecting and authenticating (unless username is null)
            _client.Connect(settings.Server, settings.Port, settings.Secure);
            try
            {
                // Checking if we should authenticate.
                if (settings.Username != null || settings.Password != null)
                    _client.Authenticate(settings.Username, settings.Password);

                // Retrieving [max] number of emails.
                var count = _client.GetMessageCount();
                for (var idx = 0; idx < count && (max == -1 || count < max); idx++)
                {
                    // Getting message, and parsing to lambda
                    var message = _client.GetMessage(idx);
                    var exe = HandleMessage(message, signaler, lambda, raw);
                    signaler.Signal("eval", exe);
                }
            }
            finally
            {
                _client.Disconnect(true);
            }
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public async Task SignalAsync(ISignaler signaler, Node input)
        {
            // Retrieving server connection settings.
            var settings = new ConnectionSettings(_configuration, input, "pop3");

            // Maximum number of emails to fetch.
            var max = input.Children.SingleOrDefault(x => x.Name == "max")?.GetEx<int>() ?? 50;
            var raw = input.Children.SingleOrDefault(x => x.Name == "raw")?.GetEx<bool>() ?? false;

            // Retrieving lambda callback for what to do for each message.
            var lambda = input.Children.FirstOrDefault(x => x.Name == ".lambda") ??
                throw new ArgumentNullException("No [.lambda] provided to [wait.mail.pop3.fetch]");

            // Connecting and authenticating (unless username is null)
            await _client.ConnectAsync(settings.Server, settings.Port, settings.Secure);
            try
            {
                // Checking if we should authenticate.
                if (settings.Username != null || settings.Password != null)
                    await _client.AuthenticateAsync(settings.Username, settings.Password);

                // Retrieving [max] number of emails.
                var count = await _client.GetMessageCountAsync();
                for (var idx = 0; idx < count && (max == -1 || count < max); idx++)
                {
                    // Getting message, and parsing to lambda
                    var message = await _client.GetMessageAsync(idx);
                    var exe = HandleMessage(message, signaler, lambda, raw);
                    await signaler.SignalAsync("eval", exe);
                }
            }
            finally
            {
                await _client.DisconnectAsync(true);
            }
        }

        #region [ -- Private helper methods -- ]

        Node HandleMessage(
            MimeMessage message,
            ISignaler signaler,
            Node lambda,
            bool raw)
        {
            var exe = lambda.Clone();
            var messageNode = new Node(".message");
            exe.Add(messageNode);
            if (raw)
            {
                // Handling message in raw format.
                messageNode.Value = message.ToString();
            }
            else
            {
                // Parsing message.
                var parseNode = new Node("", message.Body);
                signaler.Signal(".mime.parse", parseNode);

                // Adding semantically parsed message to [.message] node.
                messageNode.AddRange(parseNode.Children);
                exe.Add(messageNode);
            }
            return exe;
        }

        #endregion
    }
}
