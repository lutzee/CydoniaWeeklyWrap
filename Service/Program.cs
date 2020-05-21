using System;
using System.Diagnostics;
using System.Linq;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace Cww.Service
{
    public class Program
    {
        internal static RabbitMq RabbitMq { get; set; }

        static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json",
                            optional: true);

                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .WriteTo.Console()
                        .CreateLogger();

                    // Logging
                    services.AddLogging(loggingBuilder => { loggingBuilder.AddSerilog(); });

                    // RabbitMq
                    services.Configure<RabbitMq>(hostContext.Configuration.GetSection("RabbitMq"));

                    // Database
                    var dbConfig = hostContext.Configuration.GetSection("Database");
                    services.AddDbContext<CwwDbContext>(options =>
                        options.UseMySql(dbConfig["ConnectionString"], b => { b.MigrationsAssembly("Cww.Api"); }));

                    // Mediator
                    services.AddMediatR(typeof(Known));

                    // Mass Transit
                    services.AddMassTransit(cfg =>
                    {
                        cfg.AddConsumer<GroupRecentMusicConsumer>();
                        cfg.AddConsumer<TrackDeduplicationConsumer>();
                        cfg.AddBus(ConfigureBus);
                    });

                    // Cache
                    services.AddSingleton<ICacheProvider>(new RedisCacheProvider(hostContext.Configuration));
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

        static IBusControl ConfigureBus(IRegistrationContext<IServiceProvider> registrationContext)
        {
            RabbitMq = registrationContext.Container.GetRequiredService<IOptions<RabbitMq>>().Value;

            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(RabbitMq.Host, RabbitMq.VirtualHost, h =>
                {
                    h.Username(RabbitMq.Username);
                    h.Password(RabbitMq.Password);
                });

                cfg.ReceiveEndpoint("cww", e =>
                {
                    e.PrefetchCount = 16;
                    e.UseMessageRetry(x => x.Interval(2, 100));
                    e.ConfigureConsumer<GroupRecentMusicConsumer>(registrationContext);
                });
                cfg.ConfigureEndpoints(registrationContext);
            });
        }
    }
}