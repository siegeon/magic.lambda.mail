/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MimeKit;
using magic.node;
using magic.signals.services;
using magic.signals.contracts;
using magic.lambda.mime.contracts;
using magic.node.extensions.hyperlambda;
using Microsoft.Extensions.Configuration;

namespace magic.lambda.mail.tests
{
    public class MockSmtpClient : ISmtpClient
    {
        readonly Action<MimeMessage> _send;
        readonly Action<string, int, bool> _connect;
        readonly Action<string, string> _authenticate;

        public MockSmtpClient(
            Action<MimeMessage> send,
            Action<string, int, bool> connect = null,
            Action<string, string> authenticate = null)
        {
            _send = send ?? throw new ArgumentNullException(nameof(send));
            _connect = connect;
            _authenticate = authenticate;
        }

        public void Authenticate(string username, string password)
        {
            _authenticate?.Invoke(username, password);
        }

        public async Task AuthenticateAsync(string username, string password)
        {
            _authenticate?.Invoke(username, password);
            await Task.Yield();
        }

        public void Connect(string host, int port, bool useSsl)
        {
            _connect?.Invoke(host, port, useSsl);
        }

        public async Task ConnectAsync(string host, int port, bool useSsl)
        {
            _connect?.Invoke(host, port, useSsl);
            await Task.Yield();
        }

        public void Disconnect(bool quit)
        {
        }

        public async Task DisconnectAsync(bool quit)
        {
            await Task.Yield();
        }

        public void Dispose()
        {
        }

        public void Send(MimeMessage message)
        {
            _send(message);
        }

        public async Task SendAsync(MimeMessage message)
        {
            _send(message);
            await Task.Yield();
        }
    }

    public static class Common
    {
        static public Node Evaluate(
            string hl,
            Action<MimeMessage> send,
            Action<string, int, bool> connect = null,
            Action<string, string> authenticate = null)
        {
            var services = Initialize(send, connect, authenticate);
            var lambda = new Parser(hl).Lambda();
            var signaler = services.GetService(typeof(ISignaler)) as ISignaler;
            signaler.Signal("eval", lambda);
            return lambda;
        }

        static public async Task<Node> EvaluateAsync(
            string hl,
            Action<MimeMessage> send,
            Action<string, int, bool> connect = null,
            Action<string, string> authenticate = null)
        {
            var services = Initialize(send,connect, authenticate);
            var lambda = new Parser(hl).Lambda();
            var signaler = services.GetService(typeof(ISignaler)) as ISignaler;
            await signaler.SignalAsync("wait.eval", lambda);
            return lambda;
        }

        static public ISignaler GetSignaler(
            Action<MimeMessage> send,
            Action<string, int, bool> connect = null,
            Action<string, string> authenticate = null)
        {
            var services = Initialize(send, connect, authenticate);
            return services.GetService(typeof(ISignaler)) as ISignaler;
        }

        #region [ -- Private helper methods -- ]

        static IServiceProvider Initialize(
            Action<MimeMessage> send,
            Action<string, int, bool> connect = null,
            Action<string, string> authenticate = null)
        {
            var services = new ServiceCollection();
            services.AddTransient<ISignaler, Signaler>();
            services.AddTransient<ISmtpClient>((svc) => new MockSmtpClient(send, connect, authenticate));

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:host")]).Returns("foo2.com");
            mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:port")]).Returns("321");
            mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:secure")]).Returns("false");
            mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:username")]).Returns("xxx2");
            mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:password")]).Returns("yyy2");
            services.AddTransient((svc) => mockConfiguration.Object);

            var types = new SignalsProvider(InstantiateAllTypes<ISlot>(services));
            services.AddTransient<ISignalsProvider>((svc) => types);
            var provider = services.BuildServiceProvider();
            return provider;
        }

        static IEnumerable<Type> InstantiateAllTypes<T>(ServiceCollection services) where T : class
        {
            var type = typeof(T);
            var result = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !x.IsDynamic && !x.FullName.StartsWith("Microsoft", StringComparison.InvariantCulture))
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface && !p.IsAbstract);

            foreach (var idx in result)
            {
                services.AddTransient(idx);
            }
            return result;
        }

        #endregion
    }
}
