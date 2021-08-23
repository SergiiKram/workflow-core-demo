using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Orchestrator.Workflow.HelloWorld.ActivityStep
{
    public class WaitActivityStep : StepBody
    {
        public static readonly string EventName = "ActivityDone";

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            if (context.ExecutionPointer.EventPublished)
            {
                Console.WriteLine("Event Received");
                return ExecutionResult.Next();
            }

            Console.WriteLine("Wait for Event");

            return ExecutionResult.WaitForEvent(EventName, context.Workflow.Id, DateTime.UtcNow);
        }
    }
}
