using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Orchestrator.Workflow.HelloWorld
{
    public class GoodbyeWorldStep : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine("Goodbye world");

            return ExecutionResult.Next();
        }
    }
}
