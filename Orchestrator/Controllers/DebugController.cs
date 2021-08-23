using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orchestrator.Workflow.HelloWorld;
using System.Threading.Tasks;
using Orchestrator.Workflow.HelloWorld.ActivityStep;
using WorkflowCore.Interface;

namespace Orchestrator.Controllers
{
    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly ILogger<DebugController> _logger;
        private readonly IWorkflowController _workflowController;
        private readonly IPersistenceProvider _workflowStore;

        public DebugController(ILogger<DebugController> logger, IWorkflowController workflowController, IPersistenceProvider workflowStore)
        {
            _logger = logger;
            _workflowController = workflowController;
            _workflowStore = workflowStore;
        }

        [HttpGet("workflows/{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var result = await _workflowStore.GetWorkflowInstance(id);
            return Ok(result);
        }

        /// <summary>
        /// Trigger <c>HelloWorldWorkflow</c> manually.
        /// For debug only, use message command to trigger it.
        /// </summary>
        /// <returns></returns>
        [HttpPost("hello-world")]
        public async Task<IActionResult> HelloWorld()
        {
            var id = await _workflowController.StartWorkflow(HelloWorldWorkflow.WorkflowId);

            return Ok(new {id = id});
        }

        [HttpPost("event")]
        public async Task<IActionResult> PublishEvent(string id)
        {
            await _workflowController.PublishEvent(WaitActivityStep.EventName, id, null);

            return Ok();
        }
    }
}
