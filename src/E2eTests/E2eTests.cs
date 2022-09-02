using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using System;
using System.Threading.Tasks;
using WorkflowCore.Models;
using Xunit;

namespace E2eTests
{
    public class E2eTests
    {
        private readonly IHost _host;

        public E2eTests()
        {
            var orchestratorUrl = Environment.GetEnvironmentVariable("ORCHESTRATOR_URL")
                ?? "http://localhost:7080/";

            var builder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient<OrchestratorClient>(client =>
                {
                    client.BaseAddress = new Uri(orchestratorUrl);
                })
                .AddTransientHttpErrorPolicy(builder => builder.WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(3),
                    TimeSpan.FromSeconds(3)
                }));
            });

            _host = builder.Build();
        }

        [Fact]
        public async Task TestWorkflowIsIdempotent()
        {
            // RabbitMQ initilizes too long in GitHub actions
            if (RunsInDocker)
            {
                await Task.Delay(TimeSpan.FromSeconds(15));
            }

            var orchestratorClient = _host.Services.GetRequiredService<OrchestratorClient>();

            var reference = Guid.NewGuid().ToString();
            var id1 = await orchestratorClient.StartWorkflow(reference);
            var id2 = await orchestratorClient.StartWorkflow(reference);

            for (var i = 0; i < 5; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(15));

                var wf = await orchestratorClient.GetWorkflow(id1.Id);

                if (wf.Status == WorkflowStatus.Complete || wf.Status == WorkflowStatus.Terminated)
                {
                    break;
                }
            }

            var wf1 = await orchestratorClient.GetWorkflow(id1.Id);
            var wf2 = await orchestratorClient.GetWorkflowByReference(reference);

            Assert.Equal(id1.Id, id2.Id);
            Assert.Equal(reference, wf1.Reference);
            Assert.Equal(reference, wf2.Reference);
            Assert.Equal(WorkflowStatus.Complete, wf1.Status);
        }

        private bool RunsInDocker 
        {
            get { return string.Equals(Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"), "true", StringComparison.OrdinalIgnoreCase); }
        }
    }
}
