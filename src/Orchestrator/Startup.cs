using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Orchestrator.Workflow;
using Orchestrator.Workflow.HelloWorld;
using Orchestrator.Workflow.HelloWorld.ActivityStep;
using OrchestratorContracts;
using System;
using System.Text.Json.Serialization;

namespace Orchestrator
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

            AddWorkflow(services);

            AddMassTransit(services);

            // Increase chances of graceful shutdown for Workflow Core
            services.Configure<HostOptions>(
                opts => opts.ShutdownTimeout = TimeSpan.FromSeconds(15));

            services.AddControllers().AddJsonOptions(options =>
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orchestrator", Version = "v1" });
            });
        }

       // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orchestrator v1");
                    c.DisplayRequestDuration();
                });
            }

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
                x.AddConsumer<ActivityResultConsumer>();

                x.SetKebabCaseEndpointNameFormatter();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("rabbitmq", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    
                    //cfg.Send<StartActivityMessage>(x => x.UseRoutingKeyFormatter(context => context.Message.WorkflowId));
                    cfg.ReceiveEndpoint("activity-result", e =>
                    {
                        e.PrefetchCount = 1;
                        e.Consumer<ActivityResultConsumer>(context);
                        e.ConfigureMessageTopology<ActivityResultMessage>(false);
                    });
                });

                //x.UsingAmazonSqs((context, cfg) =>
                //{
                //    cfg.Host("us-east-2", h =>
                //    {
                //        h.AccessKey("");
                //        h.SecretKey("");
                //    });

                //    cfg.ReceiveEndpoint("activity-result", e =>
                //    {
                //        e.Consumer<ActivityResultConsumer>(context);
                //        e.ConfigureMessageTopology<ActivityResultMessage>(false);
                //    });
                //});
            });

            services.AddMassTransitHostedService();

            return services;
        }

        private static IServiceCollection AddWorkflow(IServiceCollection services)
        {
            services.AddWorkflow(cfg =>
            {
                // Migration should be separated.
                cfg.UsePostgreSQL(@"Server=postgres;Port=5432;Database=workflow;User Id=postgres;Password=password;", true, true);

                cfg.UseRedisLocking("redis:6379");
                
                // Can be Redis/SQS/RabbitMQ
                // Install-Package WorkflowCore.QueueProviders.RabbitMQ
                //cfg.UseRabbitMQ(new ConnectionFactory
                //{
                //    HostName = "rabbitmq", UserName = "guest", Password = "guest"
                //});
                // Install-Package WorkflowCore.Providers.AWS
                // cfg.UseAwsSimpleQueueService(new EnvironmentVariablesAWSCredentials(), new AmazonSQSConfig() { RegionEndpoint = RegionEndpoint.USWest2 }, "queues-prefix");
                cfg.UseRedisQueues("redis:6379", "orchestrator");

                cfg.UseRedisEventHub("redis:6379", "orchestrator-events");
            });

            services.AddWorkflowStepMiddleware<PollyRetryMiddleware>();

            services.AddTransient<HelloWorldWorkflow>();
            services.AddTransient<HelloWorldStep>();
            services.AddTransient<InvokeActivityStep>();
            services.AddTransient<WaitActivityStep>();
            services.AddTransient<GoodbyeWorldStep>();

            services.AddHostedService<WorkflowService>();

            return services;
        }
    }
}
