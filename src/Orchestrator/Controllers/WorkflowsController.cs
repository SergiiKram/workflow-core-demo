using Microsoft.AspNetCore.Mvc;
using Orchestrator.Workflow.Cancellable;
using Orchestrator.Workflow.HelloWorld;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Persistence.EntityFramework.Exceptions;
using WorkflowCore.Persistence.EntityFramework.Interfaces;

namespace Orchestrator.Controllers
{
    [ApiController]
    [Route("workflows")]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowController _workflowController;
        private readonly IExtendedPersistenceProvider _workflowStore;
        private readonly IWorkflowPurger _workflowPurger;

        public WorkflowsController(IWorkflowController workflowController, IExtendedPersistenceProvider workflowStore, IWorkflowPurger workflowPurger)
        {
            _workflowController = workflowController;
            _workflowStore = workflowStore;
            _workflowPurger = workflowPurger;
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

        [HttpPost("cancellable-workflow", Name = "StartCancellableWorkflow")]
        public async Task<WorkflowId> StartCancellableWorkflow(string reference)
        {
            try
            {
                var id = await _workflowController.StartWorkflow(CancellableWorkflow.WorkflowId, reference: reference);

                return new WorkflowId { Id = id };
            }
            catch (WorkflowExistsException)
            {
                var workflowInstance = await _workflowStore.GetWorkflowInstanceByReference(reference);

                return new WorkflowId { Id = workflowInstance.Id };
            }
        }

        [HttpDelete("{id}", Name= "CancelWorkflow")]
        public async Task CancelWorkflow(string id)
        {
            await _workflowController.PublishEvent(WaitCancelStep.EventName, id, null);
        }

        [HttpDelete(Name = "DeleteALLWorkflows")]
        public async Task DeleteALLWorkflows()
        {
            await _workflowPurger.PurgeWorkflows(WorkflowStatus.Runnable, DateTime.UtcNow);
            await _workflowPurger.PurgeWorkflows(WorkflowStatus.Suspended, DateTime.UtcNow);
            await _workflowPurger.PurgeWorkflows(WorkflowStatus.Complete, DateTime.UtcNow);
            await _workflowPurger.PurgeWorkflows(WorkflowStatus.Terminated, DateTime.UtcNow);
        }

        public record WorkflowId
        {
            public string Id { init; get; }
        }
    }
}
