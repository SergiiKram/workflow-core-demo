using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Orchestrator.Workflow.HelloWorld;
using System.Threading.Tasks;
using Orchestrator.Workflow.HelloWorld.ActivityStep;
using WorkflowCore.Interface;
using WorkflowCore.Persistence.EntityFramework.Exceptions;
using WorkflowCore.Persistence.EntityFramework.Interfaces;

namespace Orchestrator.Controllers
{
    [ApiController]
    [Route("debug")]
    public class DebugController : ControllerBase
    {
        private readonly ILogger<DebugController> _logger;
        private readonly IWorkflowController _workflowController;
        private readonly IExtendedPersistenceProvider _workflowStore;

        public DebugController(ILogger<DebugController> logger, IWorkflowController workflowController, IExtendedPersistenceProvider workflowStore)
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
        public async Task<IActionResult> HelloWorld(string reference)
        {
            try
            {
                var id = await _workflowController.StartWorkflow(HelloWorldWorkflow.WorkflowId, reference: reference);

                return Ok(new { id = id });
            }
            catch (WorkflowExistsException)
            {
                var workflowInstance = await _workflowStore.GetWorkflowInstanceByReference(reference);

                return Ok(new { id = workflowInstance.Id });
            }
        }

        [HttpPost("event")]
        public async Task<IActionResult> PublishEvent(string id)
        {
            await _workflowController.PublishEvent(WaitActivityStep.EventName, id, null);

            return Ok();
        }
    }
}
