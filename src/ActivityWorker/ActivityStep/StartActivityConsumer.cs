using MassTransit;
using OrchestratorContracts;
using System;
using System.Threading.Tasks;

namespace ActivityWorker.ActivityStep
{
    public class StartActivityConsumer : IConsumer<StartActivityMessage>
    {
        readonly ISendEndpointProvider _sendEndpoint;

        public StartActivityConsumer(ISendEndpointProvider sendEndpoint)
        {
            _sendEndpoint = sendEndpoint;
        }

        public async Task Consume(ConsumeContext<StartActivityMessage> context)
        {
            Console.WriteLine("Activity Triggered");

            await Task.Delay(TimeSpan.FromSeconds(10));

            await _sendEndpoint.Send(new ActivityResultMessage
            {
                WorkflowId = context.Message.WorkflowId, Data = "Hello"
            });
        }
    }
}
