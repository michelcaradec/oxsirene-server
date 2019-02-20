using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using API = OxSirene.API;

namespace OxSirene.AzFunc
{
    public static class GetEligibilityChecker
    {       
        [FunctionName("GetEligibilityChecker")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/product/checker")] HttpRequest req,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(GetEligibilityChecker)} HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                ConfigurationUtils.Initialize(context, log);

                string requestBody;
                using (var reader = new StreamReader(req.Body))
                {
                    requestBody = await reader.ReadToEndAsync();
                }
                log.LogDebug(requestBody);
                var request = JsonConvert.DeserializeObject<API.GetEligibilityCheckerRequest>(requestBody, JsonUtils.SerializerSettings);
                if (!(request?.IsValid ?? false))
                {
                    return new BadRequestObjectResult($"Please pass a valid {nameof(API.GetEligibilityCheckerRequest)} object");
                }

                try
                {
                    var response = await API.GetEligibilityChecker.RunAsync(request);

                    log.LogDebug(JsonConvert.SerializeObject(response));

                    return (ActionResult)new OkObjectResult(response);
                }
                catch (Exception e)
                {
                    log.LogError(e.ToString());

                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }
            }
            finally
            {
                log.LogInformation($"End {nameof(GetEligibilityChecker)} HTTP trigger.");
            }
        }
    }
}