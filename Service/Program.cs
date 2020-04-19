using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Cww.Service.Consumers;
using Cww.Service.Services;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cww.Service
{
    public class Program
    {
        internal static Config Config { get; set; }

        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile("appsettings.json", true);
                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<Config>(hostContext.Configuration.GetSection("Config"));

                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumer<GroupRecentMusicConsumer>();
                        cfg.AddBus(ConfigureBus);
                    });
                    services.AddHostedService<ConsoleService>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                });

            if (isService)
            {
                await builder.UseSystemd().Build().RunAsync();
            }
            else
            {
                await builder.RunConsoleAsync();
            }
        }

        static IBusControl ConfigureBus(IServiceProvider provider)
        {
            Config = provider.GetRequiredService<IOptions<Config>>().Value;

            X509Certificate2 x509Certificate2 = null;

            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            try
            {
                X509Certificate2Collection certificatesInStore = store.Certificates;

                x509Certificate2 = certificatesInStore.OfType<X509Certificate2>()
                    .FirstOrDefault(cert => cert.Thumbprint?.ToLower() == Config.SSLThumbprint?.ToLower());
            }
            finally
            {
                store.Close();
            }

            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(Config.Host, Config.VirtualHost, h =>
                {
                    h.Username(Config.Username);
                    h.Password(Config.Password);

                    if (Config.SSLActive)
                    {
                        h.UseSsl(ssl =>
                        {
                            ssl.ServerName = Dns.GetHostName();
                            ssl.AllowPolicyErrors(SslPolicyErrors.RemoteCertificateNameMismatch);
                            ssl.Certificate = x509Certificate2;
                            ssl.Protocol = SslProtocols.Tls12;
                            ssl.CertificateSelectionCallback = CertificateSelectionCallback;
                        });
                    }
                });

                cfg.ReceiveEndpoint("cww", e =>
                {
                    e.PrefetchCount = 16;
                    e.UseMessageRetry(x => x.Interval(2, 100));
                    e.ConfigureConsumer<GroupRecentMusicConsumer>(provider);
                });
                cfg.ConfigureEndpoints(provider);
            });
        }

        private static X509Certificate CertificateSelectionCallback(object sender,
            string targetHost,
            X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate,
            string[] acceptableIssuers)
        {
            var serverCertificate = localCertificates.OfType<X509Certificate2>()
                .FirstOrDefault(cert => cert.Thumbprint.ToLower() == Config.SSLThumbprint.ToLower());

            return serverCertificate ?? throw new Exception("Wrong certificate");
        }
    }
}