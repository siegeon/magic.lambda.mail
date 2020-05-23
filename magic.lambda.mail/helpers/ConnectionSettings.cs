/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using magic.node;
using magic.node.extensions;

namespace magic.lambda.mime.helpers
{
    /*
     * Helper class to retrieve server connection settings (POP3/SMTP) from Node, and defaulting to configuration
     * of application if not given explicitly as arguments.
     */
    public class ConnectionSettings
    {
        public ConnectionSettings(IConfiguration configuration, Node input, string serverType)
        {
            // Retrieving connection arguments.
            Server = input.Children.SingleOrDefault(x => x.Name == "server")?.GetEx<string>() ??
                configuration[$"magic:{serverType}:server"] ??
                throw new ArgumentNullException("No [server] provided");

            Port = input.Children.SingleOrDefault(x => x.Name == "port")?.GetEx<int>() ??
                (configuration[$"magic:{serverType}:port"] != null ? new int?(int.Parse(configuration[$"magic:{serverType}:port"])) : null) ??
                throw new ArgumentNullException("No [port] provided to [wait.mail.pop3.fetch]");

            Secure = input.Children.SingleOrDefault(x => x.Name == "secure")?.GetEx<bool>() ??
                (configuration[$"magic:{serverType}:secure"] != null ? new bool?(bool.Parse(configuration[$"magic:{serverType}:secure"])) : null) ??
                false;

            Username = input.Children.SingleOrDefault(x => x.Name == "username")?.GetEx<string>() ??
                configuration[$"magic:{serverType}:username"];

            Password = input.Children.SingleOrDefault(x => x.Name == "password")?.GetEx<string>() ??
                configuration[$"magic:{serverType}:password"];
        }

        public string Server { get; private set; }

        public int Port { get; private set; }

        public bool Secure { get; private set; }

        public string Username { get; private set; }

        public string Password { get; private set; }
    }
}
