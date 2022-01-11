using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Orchestrator.Workflow.Cancellable
{
    internal class CancelData
    {
        public bool Cancel { get; set; }

        public bool Done { get; set; }
    }

    internal class CancellableWorkflow : IWorkflow<CancelData>
    {
        public static string WorkflowId = "CancellableWorkflow";
        public string Id => WorkflowId;

        public int Version => 2;

        public void Build(IWorkflowBuilder<CancelData> builder)
        {
            builder
                .UseDefaultErrorBehavior(WorkflowErrorHandling.Terminate)
                .StartWith<PrintMessageStep>()
                    .Input(step => step.Message, data => "Hello Cancellable Workflow!")

                .Parallel()

                .Do(then => then
                    .StartWith<PrintMessageStep>()
                        .Input(step => step.Message, data => "Hello normal path!")
                    .Then<LongStep>()
                    .Then<PrintMessageStep>()
                        .Input(step => step.Message, data => "Long operation completed.")
                    .Then<PrintMessageStep>()
                        .Input(step => step.Message, data => "Another action after long operation.")
                        .Output(data => data.Done, step => true)
                    )

                .Do(then =>
                    then.StartWith<PrintMessageStep>()
                        .Input(step => step.Message, data => "Hello cancel path!")
                    .Then<WaitCancelStep>()
                        .Output(data => data.Cancel, step => true)
                    )

                .Join()
                .CancelCondition(data => data.Cancel || data.Done, true)

                .Then<PrintMessageStep>()
                    .Input(step => step.Message, data => $"Workflow finished: Done: {data.Done}, Cancel: {data.Cancel}");
        }
    }

    internal class PrintMessageStep : StepBody
    {
        public string Message { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine(Message);
            return ExecutionResult.Next();
        }
    }

    internal class LongStep : StepBodyAsync
    {
        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine("Long operations start!");
            
            await Task.Delay(TimeSpan.FromSeconds(40));
            
            Console.WriteLine("Long operations done!");

            return ExecutionResult.Next();
        }
    }

    internal class WaitCancelStep : StepBody
    {
        public static readonly string EventName = "WaitCancelStep";

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            if (context.ExecutionPointer.EventPublished)
            {
                Console.WriteLine("Cancel Received");
                return ExecutionResult.Next();
            }

            Console.WriteLine("Wait for Cancel");

            return ExecutionResult.WaitForEvent(EventName, context.Workflow.Id, DateTime.UtcNow);
        }
    }
}
