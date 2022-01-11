using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace Orchestrator.Workflow
{
    public class PollyRetryMiddleware : IWorkflowStepMiddleware
    {
        private const string StepContextKey = "WorkflowStepContext";
        private const int MaxRetries = 3;
        private readonly ILogger<PollyRetryMiddleware> _log;

        public PollyRetryMiddleware(ILogger<PollyRetryMiddleware> log)
        {
            _log = log;
        }

        public async Task<ExecutionResult> HandleAsync(
            IStepExecutionContext context,
            IStepBody body,
            WorkflowStepDelegate next
        )
        {
            return await GetRetryPolicy().ExecuteAsync(ctx => next(), new Dictionary<string, object>
            {
                { StepContextKey, context }
            });
        }

        private IAsyncPolicy<ExecutionResult> GetRetryPolicy() =>
            Policy<ExecutionResult>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    MaxRetries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (result, sleepDuration, retryCount, context) =>
                        UpdateRetryCount(result.Exception, retryCount, context[StepContextKey] as IStepExecutionContext)
                );

        private Task UpdateRetryCount(
            Exception exception,
            int retryCount,
            IStepExecutionContext stepContext)
        {
            var stepInstance = stepContext.ExecutionPointer;
            stepInstance.RetryCount = retryCount;

            _log.LogWarning(
                exception,
                "Exception occurred in step {StepId}. Retrying [{RetryCount}/{MaxCount}]",
                stepInstance.Id,
                retryCount,
                MaxRetries
            );

            return Task.CompletedTask;
        }
    }
}
