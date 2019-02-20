using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using API = OxSirene.API;
using System.Net.Http;

namespace OxSirene.AzFunc
{
    public static class ScrapSeller
    {
        [FunctionName("ScrapSeller_GET")]
        public static async Task<IActionResult> Run_GET(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/seller/scrap/{marketplace}/{seller}")] HttpRequestMessage req,
            string marketplace,
            string seller,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(ScrapSeller)}_GET HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                return await Run_Impl(new API.ScrapSellerRequest(marketplace, seller, false), log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(ScrapProduct)}_GET HTTP trigger.");
            }
        }

        [FunctionName("ScrapSeller_POST")]
        public static async Task<IActionResult> Run_POST(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/seller/scrap")] HttpRequestMessage req,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(ScrapSeller)}_POST HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                string requestBody = await req.Content.ReadAsStringAsync();
                log.LogDebug(requestBody);
                var request = JsonConvert.DeserializeObject<API.ScrapSellerRequest>(requestBody, JsonUtils.SerializerSettings);

                return await Run_Impl(request, log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(ScrapProduct)}_POST HTTP trigger.");
            }
        }

        private static async Task<IActionResult> Run_Impl(
            API.ScrapSellerRequest request,
            ILogger log,
            ExecutionContext context
        )
        {
            ConfigurationUtils.Initialize(context, log);

            if (!(request?.IsValid ?? false))
            {
                return new BadRequestObjectResult($"Please pass a valid {nameof(API.ScrapSellerRequest)} object");
            }

            try
            {
                var response = await API.ScrapSeller.RunAsync(request);

                log.LogDebug(JsonConvert.SerializeObject(response));

                return (ActionResult)new OkObjectResult(response);
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());

                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
