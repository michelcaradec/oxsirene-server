using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OxSirene.AzFunc
{
    public static class RequestAPIAccessToken
    {
        [FunctionName("RequestAPIAccessToken")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/token")] HttpRequest req,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(RequestAPIAccessToken)} HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                ConfigurationUtils.Initialize(context, log);

                return await Task.Run(
                    () => (ActionResult)new OkObjectResult(new { key = API.Configuration.Instance.APIAccessToken })
                );
            }
            finally
            {
                log.LogInformation($"End {nameof(RequestAPIAccessToken)} HTTP trigger.");
            }
        }
    }
}
