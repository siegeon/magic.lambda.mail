/*
 * Magic, Copyright(c) Thomas Hansen 2019 - 2020, thomas@servergardens.com, all rights reserved.
 * See the enclosed LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MimeKit;
using magic.node;
using magic.signals.services;
using magic.signals.contracts;
using magic.lambda.mime.contracts;
using magic.lambda.mail.tests.helpers;
using magic.node.extensions.hyperlambda;

namespace magic.lambda.mail.tests
{
    public static class Common
    {
        static public Node Evaluate(
            string hl,
            MockSmtpClient smtp,
            MockPop3Client pop3 = null)
        {
            var services = Initialize(smtp, pop3);
            var lambda = new Parser(hl).Lambda();
            var signaler = services.GetService(typeof(ISignaler)) as ISignaler;
            signaler.Signal("eval", lambda);
            return lambda;
        }

        static public async Task<Node> EvaluateAsync(
            string hl,
            MockSmtpClient smtp,
            MockPop3Client pop3 = null)
        {
            var services = Initialize(smtp, pop3);
            var lambda = new Parser(hl).Lambda();
            var signaler = services.GetService(typeof(ISignaler)) as ISignaler;
            await signaler.SignalAsync("wait.eval", lambda);
            return lambda;
        }

        static public ISignaler GetSignaler(MockSmtpClient smtp, MockPop3Client pop3)
        {
            var services = Initialize(smtp, pop3);
            return services.GetService(typeof(ISignaler)) as ISignaler;
        }

        #region [ -- Private helper methods -- ]

        static IServiceProvider Initialize(MockSmtpClient smtp, MockPop3Client pop3)
        {
            var services = new ServiceCollection();
            services.AddTransient<ISignaler, Signaler>();
            var mockConfiguration = new Mock<IConfiguration>();
            if (smtp != null)
            {
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:host")]).Returns("foo2.com");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:port")]).Returns("321");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:secure")]).Returns("false");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:username")]).Returns("xxx2");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:password")]).Returns("yyy2");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:from:name")]).Returns("Foo Bar");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:smtp:from:address")]).Returns("foo@bar.com");
                services.AddTransient<ISmtpClient>((svc) => smtp);
            }
            if (pop3 != null)
            {
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:pop3:host")]).Returns("foo2.com");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:pop3:port")]).Returns("321");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:pop3:secure")]).Returns("false");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:pop3:username")]).Returns("xxx2");
                mockConfiguration.SetupGet(x => x[It.Is<string>(x => x == "magic:pop3:password")]).Returns("yyy2");
                services.AddTransient<IPop3Client>((svc) => pop3);
            }
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
