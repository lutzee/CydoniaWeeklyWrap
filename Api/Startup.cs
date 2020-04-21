using Cww.Core;
using Cww.Core.Database;
using Cww.Core.Factories;
using Cww.Core.Messages;
using IF.Lastfm.Core.Api;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Cww.Api
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.RollingFile("Logs/log-{Date}.txt", retainedFileCountLimit: 3)
                .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger.Information("System starting");
            services.AddRazorPages();
            
            var lastAuth = new LastAuth(Configuration["LastFm:ApiKey"], Configuration["LastFm:ApiSecret"]);
            
            services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    builder => { builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod(); });
            });

            services.AddMemoryCache();
            services.AddControllers();

            // Database
            var dbConfig = Configuration.GetSection("Database");
            services.AddDbContext<CwwDbContext>(options => options.UseMySql(dbConfig["ConnectionString"], b =>
            {
                b.MigrationsAssembly("Cww.Api");
            }));

            // Mediator
            services.AddMediatR(typeof(Known));

            // Last.FM
            services.AddSingleton<ILastAuth>(lastAuth);
            services.AddTransient<IUserApi, UserApi>();
            services.AddTransient<ITrackApi, TrackApi>();

            // Spotify
            services.AddTransient<ISpotifyApiFactory, SpotifyApiFactory>();

            // MassTransit
            services.AddMassTransit(x =>
            {
                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    // configure health checks for this bus instance
                    cfg.UseHealthCheck(provider);

                    cfg.Host("localhost", "cww", c =>
                    {
                        c.Username("guest");
                        c.Password("guest");
                    });
                }));

                x.AddRequestClient<GroupRecentMusic.Request>();
                x.AddRequestClient<TrackDeduplication.Request>();
            });
            services.AddHealthChecks();
            services.AddMassTransitHostedService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, CwwDbContext dbContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            
            app.UseCors(MyAllowSpecificOrigins);

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            dbContext.Database.Migrate();
        }
    }
}
