/*
 * Magic Cloud, copyright Aista, Ltd. See the attached LICENSE file for details.
 */

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using magic.node;
using magic.node.services;
using magic.node.contracts;
using magic.signals.services;
using magic.signals.contracts;
using magic.lambda.mime.contracts;
using magic.lambda.mail.tests.helpers;
using magic.node.extensions.hyperlambda;

namespace magic.lambda.mail.tests
{
    public static class Common
    {
        private class RootResolver : IRootResolver
        {
            public string DynamicFiles => AppDomain.CurrentDomain.BaseDirectory;
            public string RootFolder => AppDomain.CurrentDomain.BaseDirectory;

            public string AbsolutePath(string path)
            {
                return DynamicFiles + path.TrimStart(new char[] { '/', '\\' });
            }

            public string RelativePath(string path)
            {
                return path.Substring(DynamicFiles.Length - 1);
            }
        }

        static public Node Evaluate(
            string hl,
            MockSmtpClient smtp = null,
            MockPop3Client pop3 = null)
        {
            var services = Initialize(smtp, pop3);
            var lambda = HyperlambdaParser.Parse(hl);
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
            var lambda = HyperlambdaParser.Parse(hl);
            var signaler = services.GetService(typeof(ISignaler)) as ISignaler;
            await signaler.SignalAsync("eval", lambda);
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
            services.AddTransient<IRootResolver, RootResolver>();
            var mockConfiguration = new Mock<IMagicConfiguration>();
            if (smtp != null)
            {
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:smtp:host")]).Returns("foo2.com");
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:smtp:port")]).Returns("321");
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:smtp:secure")]).Returns("false");
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:smtp:username")]).Returns("xxx2");
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:smtp:password")]).Returns("yyy2");
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:smtp:from:name")]).Returns("Foo Bar");
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:smtp:from:address")]).Returns("foo@bar.com");
                services.AddTransient<ISmtpClient>((svc) => smtp);
            }
            if (pop3 != null)
            {
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:pop3:host")]).Returns("foo2.com");
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:pop3:port")]).Returns("321");
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:pop3:secure")]).Returns("false");
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:pop3:username")]).Returns("xxx2");
                mockConfiguration.SetupGet(x => x[It.Is<string>(y => y == "magic:pop3:password")]).Returns("yyy2");
                services.AddTransient<IPop3Client>((svc) => pop3);
            }
            services.AddTransient((svc) => mockConfiguration.Object);

            var types = new SignalsProvider(InstantiateAllTypes<ISlot>(services));
            services.AddTransient<ISignalsProvider>((svc) => types);
            services.AddTransient<IStreamService, StreamService>();
            return services.BuildServiceProvider();
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
