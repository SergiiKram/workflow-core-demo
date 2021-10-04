using ActivityWorker.ActivityStep;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OrchestratorContracts;
using System;

namespace ActivityWorker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();

            services.Configure<HealthCheckPublisherOptions>(options =>
            {
                options.Delay = TimeSpan.FromSeconds(2);
                options.Predicate = (check) => check.Tags.Contains("ready");
            });

            AddMassTransit(services);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions()
                {
                    Predicate = (check) => check.Tags.Contains("ready"),
                });

                endpoints.MapHealthChecks("/health/live", new HealthCheckOptions());

                endpoints.MapControllers();
            });
        }

        private static IServiceCollection AddMassTransit(IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<StartActivityConsumer>();

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    EndpointConvention.Map<ActivityResultMessage>(new Uri("queue:activity-result"));

                    cfg.ReceiveEndpoint("start-activity", e =>
                    {
                        e.ConcurrentMessageLimit = 1;
                        e.PrefetchCount = 1;
                        e.Consumer<StartActivityConsumer>(context);
                        //e.ConfigureMessageTopology<StartActivityMessage>(false);
                    });
                });

                //x.UsingAmazonSqs((context, cfg) =>
                //{
                //    cfg.Host("us-east-2", h =>
                //    {
                //        h.AccessKey("");
                //        h.SecretKey("");
                //    });

                //    EndpointConvention.Map<ActivityResultMessage>(new Uri("queue:activity-result"));

                //    cfg.ReceiveEndpoint("start-activity", e =>
                //    {
                //        e.Consumer<StartActivityConsumer>(context);
                //    });
                //});
            });

            services.AddMassTransitHostedService();

            return services;
        }
    }
}
