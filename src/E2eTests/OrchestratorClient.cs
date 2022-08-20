using Flurl;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WorkflowCore.Models;

namespace E2eTests
{
    class OrchestratorClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;

        public OrchestratorClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _serializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public async Task<WorkflowId> StartWorkflow(string reference)
        {
            var uri = new Url(_httpClient.BaseAddress)
                .AppendPathSegments("workflows", "hello-world")
                .SetQueryParam("reference", reference);

            var response = await _httpClient.PostAsync(uri, null);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            var workflowInstance = JsonSerializer.Deserialize<WorkflowId>(responseString, _serializerOptions);

            return workflowInstance;
        }

        public async Task<WorkflowInstance> GetWorkflowByReference(string reference)
        {
            var uri = new Url(_httpClient.BaseAddress)
                .AppendPathSegment("workflows")
                .SetQueryParam("reference", reference);

            var responseString = await _httpClient.GetStringAsync(uri);

            var workflowInstance = JsonSerializer.Deserialize<WorkflowInstance>(responseString, _serializerOptions);

            return workflowInstance;
        }

        public async Task<WorkflowInstance> GetWorkflow(string id)
        {
            var uri = new Url(_httpClient.BaseAddress)
                .AppendPathSegments("workflows", id);

            var responseString = await _httpClient.GetStringAsync(uri);

            var workflowInstance = JsonSerializer.Deserialize<WorkflowInstance>(responseString, _serializerOptions);

            return workflowInstance;
        }

        public record WorkflowId
        {
            public string Id { init; get; }
        }
    }
}
