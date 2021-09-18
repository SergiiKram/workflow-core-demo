using MassTransit;
using OrchestratorContracts;
using System;
using System.Threading.Tasks;

namespace ActivityWorker.ActivityStep
{
    public class InvokeActivityConsumer : IConsumer<ActivityMessage>
    {
        readonly IPublishEndpoint _publishEndpoint;

        public InvokeActivityConsumer(IPublishEndpoint publishEndpoint)
        {
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<ActivityMessage> context)
        {
            Console.WriteLine("Activity Triggered");

            // TODO: Use Send
            await _publishEndpoint.Publish(new ActivityResult
            {
                WorkflowId = context.Message.WorkflowId, Data = "Hello"
            });
        }
    }
}
