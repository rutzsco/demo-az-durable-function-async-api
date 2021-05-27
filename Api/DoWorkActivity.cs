using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace demo_az_durable_function_async_api
{
    public static class DoWorkActivity
    {
        [FunctionName("DoWorkActivity")]
        public static string Run([ActivityTrigger] int waitSeconds, ILogger log)
        {
            Thread.Sleep(waitSeconds * 1000);
            return $"Payload";
        }

        [FunctionName("Workflow")]
        public static async Task<List<string>> RunOrchestrator([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();
            var duration = context.GetInput<int>();
            outputs.Add(await context.CallActivityAsync<string>("DoWorkActivity", duration / 3));
            outputs.Add(await context.CallActivityAsync<string>("DoWorkActivity", duration / 3));
            outputs.Add(await context.CallActivityAsync<string>("DoWorkActivity", duration / 3));
            return outputs;
        }
    }
}