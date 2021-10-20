using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using WorkflowCore.Models;
using Xunit;

namespace E2eTests
{
    public class E2eTests
    {
        [Fact]
        public async Task IdempotentE2eTest()
        {
            var builder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient<OrchestratorClient>();
            });

            var host = builder.Build();

            var orchestratorClient = host.Services.GetRequiredService<OrchestratorClient>();

            var reference = Guid.NewGuid().ToString();
            var id1 = await orchestratorClient.StartWorkflow(reference);
            var id2 = await orchestratorClient.StartWorkflow(reference);

            await Task.Delay(TimeSpan.FromSeconds(45));

            var wf1 = await orchestratorClient.GetWorkflow(id1.Id);
            var wf2 = await orchestratorClient.GetWorkflowByReference(reference);

            Assert.Equal(id1.Id, id2.Id);
            Assert.Equal(reference, wf1.Reference);
            Assert.Equal(reference, wf2.Reference);
            Assert.Equal(WorkflowStatus.Complete, wf1.Status);
        }
    }
}
