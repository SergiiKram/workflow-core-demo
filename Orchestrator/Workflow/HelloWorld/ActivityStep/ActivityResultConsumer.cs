﻿using MassTransit;
using OrchestratorContracts;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;

namespace Orchestrator.Workflow.HelloWorld.ActivityStep
{
    public class ActivityResultConsumer : IConsumer<ActivityResult>
    {
        private readonly IWorkflowController _workflowController;

        public ActivityResultConsumer(IWorkflowController workflowController)
        {
            _workflowController = workflowController;
        }

        public async Task Consume(ConsumeContext<ActivityResult> context)
        {
            Console.WriteLine("Activity Result");

            await _workflowController.PublishEvent(WaitActivityStep.EventName, context.Message.WorkflowId, null);
        }
    }
}
