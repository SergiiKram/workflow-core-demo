using Orchestrator.Workflow.HelloWorld.ActivityStep;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Orchestrator.Workflow.HelloWorld
{
    public class HelloWorldWorkflow : IWorkflow
    {
        public static string WorkflowId = "HelloWorld";
        public string Id => WorkflowId;

        public int Version => 1;

        public void Build(IWorkflowBuilder<object> builder)
        {
            builder
                .UseDefaultErrorBehavior(WorkflowErrorHandling.Terminate)
                .StartWith<HelloWorldStep>()
                .Then<InvokeActivityStep>()
                //.WaitFor(WaitActivityStep.EventName, (o, context) => context.Workflow.Id)
                .Then<WaitActivityStep>()
                .Then<GoodbyeWorldStep>();
        }
    }
}
