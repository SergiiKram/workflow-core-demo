using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web;
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
            var uri = AddParameter("http://localhost:7080/workflows/hello-world", "reference", reference);

            var response = await _httpClient.PostAsync(uri, null);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            var workflowInstance = JsonSerializer.Deserialize<WorkflowId>(responseString, _serializerOptions);

            return workflowInstance;
        }

        public async Task<WorkflowInstance> GetWorkflowByReference(string reference)
        {
            var uri = AddParameter("http://localhost:7080/workflows", "reference", reference);

            var responseString = await _httpClient.GetStringAsync(uri);

            var workflowInstance = JsonSerializer.Deserialize<WorkflowInstance>(responseString, _serializerOptions);

            return workflowInstance;
        }

        public async Task<WorkflowInstance> GetWorkflow(string id)
        {
            var uri = new Uri(new Uri("http://localhost:7080/workflows/", UriKind.Absolute), new Uri(id, UriKind.Relative));

            var responseString = await _httpClient.GetStringAsync(uri);

            var workflowInstance = JsonSerializer.Deserialize<WorkflowInstance>(responseString, _serializerOptions);

            return workflowInstance;
        }

        public Uri AddParameter(string url, string paramName, string paramValue)
        {
            var uriBuilder = new UriBuilder(url);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query[paramName] = paramValue;
            uriBuilder.Query = query.ToString();

            return uriBuilder.Uri;
        }

        public record WorkflowId
        {
            public string Id { init; get; }
        }
    }
}
