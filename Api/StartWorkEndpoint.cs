using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace demo_az_durable_function_async_api
{
    public static class StartWorkEndpoint
    {
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


        [FunctionName("StartWorkEndpoint")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
                                                    [DurableClient] IDurableOrchestrationClient starter,
                                                    ILogger log)
        {

            var duration = ParseDuration(req);

            string instanceId = await starter.StartNewAsync("Workflow", null, duration);
            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            return starter.CreateCheckStatusResponse(req, instanceId);
        }

        private static int ParseDuration(HttpRequest req)
        {
            var duration = req.Query["duration"];
            if (string.IsNullOrEmpty(duration))
                return 300;
            try
            {
                int result = Int32.Parse(duration);
                return result;
            }
            catch (FormatException)
            {
                return 60;
            }
        }
    }
}