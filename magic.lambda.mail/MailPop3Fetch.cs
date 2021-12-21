/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using MimeKit;
using magic.node;
using magic.node.contracts;
using magic.node.extensions;
using magic.signals.contracts;
using magic.lambda.mail.helpers;
using contracts = magic.lambda.mime.contracts;

namespace magic.lambda.mail
{
    /// <summary>
    /// Fetches all new messages from the specified POP3 server/account.
    /// </summary>
    [Slot(Name = "mail.pop3.fetch")]
    public class MailPop3Fetch : ISlotAsync, ISlot
    {
        readonly IMagicConfiguration _configuration;
        readonly contracts.IPop3Client _client;
        readonly Func<int, int, int, bool> Done = (idx, count, max) => idx < count && (max == -1 || idx < max);

        /// <summary>
        /// Constructor for your class.
        /// </summary>
        /// <param name="configuration">Configuration dependency injected argument.</param>
        /// <param name="client">POP3 client implementation</param>
        public MailPop3Fetch(IMagicConfiguration configuration, contracts.IPop3Client client)
        {
            _configuration = configuration;
            _client = client;
        }

        /// <summary>
        /// Implementation of your slot.
        /// </summary>
        /// <param name="signaler">Signaler that raised the signal.</param>
        /// <param name="input">Arguments to your slot.</param>
        public void Signal(ISignaler signaler, Node input)
        {
            var settings = new Pop3Settings(input, _configuration);
            _client.Connect(
                settings.Connection.Server,
                settings.Connection.Port,
                settings.Connection.Secure);
            try
            {
                if (settings.Connection.HasCredentials)
                    _client.Authenticate(settings.Connection.Username, settings.Connection.Password);

                var count = _client.GetMessageCount();
                for (var idx = 0; Done(idx, count, settings.Max); idx++)
                {
                    var lambda = settings.Lambda.Clone();
                    var message = _client.GetMessage(idx);
                    HandleMessage(
                        message,
                        signaler,
                        lambda,
                        settings.Raw);
                    signaler.Signal("eval", lambda);
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
            var settings = new Pop3Settings(input, _configuration);
            await _client.ConnectAsync(
                settings.Connection.Server,
                settings.Connection.Port,
                settings.Connection.Secure);
            try
            {
                if (settings.Connection.HasCredentials)
                    await _client.AuthenticateAsync(settings.Connection.Username, settings.Connection.Password);

                var count = await _client.GetMessageCountAsync();
                for (var idx = 0; Done(idx, count, settings.Max); idx++)
                {
                    var lambda = settings.Lambda.Clone();
                    var message = await _client.GetMessageAsync(idx);
                    HandleMessage(
                        message,
                        signaler,
                        lambda,
                        settings.Raw);
                    await signaler.SignalAsync("eval", lambda);
                }
            }
            finally
            {
                await _client.DisconnectAsync(true);
            }
        }

        #region [ -- Private helper methods and classes -- ]

        /*
         * Helper class to encapsulate POP3 settings, such as connection settings, and other
         * types of configurations, such as how many messages to retrieve, etc.
         */
        private sealed class Pop3Settings
        {
            public Pop3Settings(Node input, IMagicConfiguration configuration)
            {
                Connection = new ConnectionSettings(
                    configuration,
                    input.Children.FirstOrDefault(x => x.Name == "server"),
                    "pop3");

                Max = input.Children.SingleOrDefault(x => x.Name == "max")?.GetEx<int>() ?? 50;
                Raw = input.Children.SingleOrDefault(x => x.Name == "raw")?.GetEx<bool>() ?? false;
                Lambda = input.Children.FirstOrDefault(x => x.Name == ".lambda") ??
                    throw new HyperlambdaException("No [.lambda] provided to [mail.pop3.fetch]");
            }

            public ConnectionSettings Connection { get; private set; }

            public int Max { get; private set; }

            public bool Raw { get; private set; }

            public Node Lambda { get; private set; }
        }

        /*
         * Helper method to handle one single message, by parsing it (unless raw is true), and invoking [.lambda]
         * callback to notify client of message retrieved.
         */
        void HandleMessage(
            MimeMessage message,
            ISignaler signaler,
            Node lambda,
            bool raw)
        {
            using (var body = message.Body)
            {
                var messageNode = new Node(".message");
                lambda.Insert(0, messageNode);

                if (raw)
                {
                    messageNode.Value = message.ToString();
                }
                else
                {
                    messageNode.Add(new Node("subject", message.Subject));
                    AddRecipient(message.From.Select(x => x as MailboxAddress), messageNode, "from");
                    AddRecipient(message.To.Select(x => x as MailboxAddress), messageNode, "to");
                    AddRecipient(message.Cc.Select(x => x as MailboxAddress), messageNode, "cc");
                    AddRecipient(message.Bcc.Select(x => x as MailboxAddress), messageNode, "bcc");

                    var parseNode = new Node("", body);
                    signaler.Signal(".mime.parse", parseNode);
                    var entity = new Node("entity", parseNode.Value);
                    entity.AddRange(parseNode.Children);
                    messageNode.Add(entity);
                }
            }
        }

        /*
         * Helper method to handle a specific type of recipient, and creating a lambda list of nodes,
         * wrapping recipient's email address.
         */
        void AddRecipient(IEnumerable<MailboxAddress> items, Node node, string nodeName)
        {
            if (items == null || !items.Any())
                return;
            var collectionNode = new Node(nodeName);
            foreach (var idx in items)
            {
                if (idx == null)
                    continue; // Might be other types of addresses in theory ...
                collectionNode.Add(new Node(idx.Name, idx.Address));
            }
            node.Add(collectionNode);
        }

        #endregion
    }
}
