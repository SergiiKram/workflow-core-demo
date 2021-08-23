﻿using MassTransit;
using OrchestratorContracts;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Orchestrator.Workflow.HelloWorld.ActivityStep
{
    public class InvokeActivityStep : StepBodyAsync
    {
        readonly IPublishEndpoint _publishEndpoint;

        public InvokeActivityStep(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            Console.WriteLine("Invoke Activity");

            // TODO: Use Send
            await _publishEndpoint.Publish(new ActivityMessage {WorkflowId = context.Workflow.Id});

            return ExecutionResult.Next();
        }
    }
}
