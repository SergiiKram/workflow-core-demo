using Microsoft.Extensions.Hosting;
using Orchestrator.Workflow.HelloWorld;
using System.Threading;
using System.Threading.Tasks;
using WorkflowCore.Interface;

namespace Orchestrator.Workflow
{
    public sealed class WorkflowService : IHostedService
    {
        private readonly IWorkflowHost _workflowHost;

        public WorkflowService(IWorkflowHost workflowHost)
        {
            _workflowHost = workflowHost;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _workflowHost.RegisterWorkflow<HelloWorldWorkflow>();
            _workflowHost.Start();

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _workflowHost.Stop();

            return Task.CompletedTask;
        }
    }
}
