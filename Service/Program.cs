using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Cww.Core;
using Cww.Core.Database;
using Cww.Core.Factories;
using Cww.Service.Cache;
using Cww.Service.Consumers;
using Cww.Service.Services;
using GreenPipes;
using IF.Lastfm.Core.Api;
using MassTransit;
using MediatR;
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
                    // Config
                    services.Configure<Config>(hostContext.Configuration.GetSection("Config"));

                    // Database
                    services.AddDbContext<CwwDbContext>();

                    // Mediator
                    services.AddMediatR(typeof(Known));

                    // Mass Transit
                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumer<GroupRecentMusicConsumer>();
                        cfg.AddBus(ConfigureBus);
                    });

                    // Cache
                    services.AddSingleton<ICacheProvider>(new RedisCacheProvider());
                    services.AddTransient<ICacheTimeoutProvider, CacheTimeoutProvider>();

                    // Apis
                    var lastFmConfig = hostContext.Configuration.GetSection("LastFm");
                    var lastAuth = new LastAuth(lastFmConfig["ApiKey"], lastFmConfig["ApiSecret"]);
                    services.AddSingleton<ILastAuth>(lastAuth);
                    services.AddTransient<ISpotifyApiFactory, SpotifyApiFactory>();
                    services.AddTransient<IUserApi, UserApi>();
                    services.AddTransient<ITrackApi, TrackApi>();

                    // Hosted services
                    services.AddHostedService<ConsoleService>();
                    services.AddHostedService<LastFmRecentMusicService>();
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