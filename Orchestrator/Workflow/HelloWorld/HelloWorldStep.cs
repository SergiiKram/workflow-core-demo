using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Orchestrator.Workflow.HelloWorld
{
    public class HelloWorldStep : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Console.WriteLine("Hello world");
            return ExecutionResult.Next();
        }
    }
}
