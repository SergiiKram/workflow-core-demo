using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Orchestrator.Workflow.HelloWorld;
using Orchestrator.Workflow.HelloWorld.ActivityStep;
using OrchestratorContracts;
using System;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Models;
using WorkflowCore.Testing;
using Xunit;

namespace Orchestrator.UnitTests
{
    public class HelloWorldWorkflowTests : WorkflowTest<HelloWorldWorkflow, object>
    {
        private readonly Mock<IPublishEndpoint> _publishEndpointMock = new Mock<IPublishEndpoint>();

        public HelloWorldWorkflowTests()
        {
            Setup();
        }

        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddLogging(x =>
            {
                x.SetMinimumLevel(LogLevel.Debug);
                x.AddDebug();
            });

            services.AddTransient<HelloWorldStep>();
            services.AddTransient<InvokeActivityStep>();
            services.AddTransient<WaitActivityStep>();
            services.AddTransient<GoodbyeWorldStep>();

            services.AddSingleton(_publishEndpointMock.Object);
        }

        [Fact]
        public async Task WorkflowTest()
        {
            var workflowId = await StartWorkflowAsync(new object());

            WaitForEventSubscription(WaitActivityStep.EventName, workflowId, TimeSpan.FromSeconds(30));
            await Host.PublishEvent(WaitActivityStep.EventName, workflowId, null);
            
            await WaitForWorkflowToCompleteAsync(workflowId, TimeSpan.FromSeconds(30));

            var status = GetStatus(workflowId);

            UnhandledStepErrors.Should().BeEmpty();
            status.Should().Be(WorkflowStatus.Complete);

            _publishEndpointMock.Verify(
                x => x.Publish(It.IsAny<StartActivityMessage>(), It.IsAny<CancellationToken>()),
                Times.Once());
        }
    }
}