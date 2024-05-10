using Microsoft.AspNetCore.Mvc;
using Orchestrator.Workflow.HelloWorld;
using System.Threading.Tasks;
using WorkflowCore.Exceptions;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Persistence.EntityFramework.Interfaces;

namespace Orchestrator.Controllers
{
    [ApiController]
    [Route("workflows")]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowController _workflowController;
        private readonly IExtendedPersistenceProvider _workflowStore;

        public WorkflowsController(IWorkflowController workflowController, IExtendedPersistenceProvider workflowStore)
        {
            _workflowController = workflowController;
            _workflowStore = workflowStore;
        }

        [HttpGet("{id}", Name="GetWorkflowById")]
        public async Task<WorkflowInstance> Get(string id)
        {
            var result = await _workflowStore.GetWorkflowInstance(id);
            return result;
        }

        [HttpGet(Name = "GetWorkflowByReference")]
        public async Task<WorkflowInstance> GetByReference([FromQuery] string reference)
        {
            var result = await _workflowStore.GetWorkflowInstanceByReference(reference);
            return result;
        }

        /// <summary>
        /// Trigger <c>HelloWorldWorkflow</c> manually.
        /// For debug only, use message command to trigger it.
        /// </summary>
        /// <returns></returns>
        [HttpPost("hello-world", Name="StartWorkflow")]
        public async Task<WorkflowId> HelloWorld(string reference)
        {
            try
            {
                var id = await _workflowController.StartWorkflow(HelloWorldWorkflow.WorkflowId, reference: reference);

                return new WorkflowId { Id = id };
            }
            catch (WorkflowExistsException)
            {
                var workflowInstance = await _workflowStore.GetWorkflowInstanceByReference(reference);

                return new WorkflowId { Id = workflowInstance.Id };
            }
        }

        public record WorkflowId
        {
            public string Id { init; get; }
        }
    }
}
