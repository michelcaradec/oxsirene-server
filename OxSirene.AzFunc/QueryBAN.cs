using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;

namespace OxSirene.AzFunc
{
    public static class QueryBAN
    {
        [FunctionName("QueryBAN_GET")]
        public static async Task<IActionResult> Run_GET(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "V1/ban/{lon}/{lat}")] HttpRequestMessage req,
            double lon,
            double lat,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(QueryBAN)}_GET HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                var request = new API.QueryBANRequest(new API.GeoPoint(lon, lat));
                return await Run_Impl(request, log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(QueryBAN)}_GET HTTP trigger.");
            }
        }

        [FunctionName("QueryBAN_POST")]
        public static async Task<IActionResult> Run_POST(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/ban")] HttpRequestMessage req,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(QueryBAN)}_POST HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                string requestBody = await req.Content.ReadAsStringAsync();
                log.LogDebug(requestBody);
                var request = JsonConvert.DeserializeObject<API.QueryBANRequest>(requestBody, JsonUtils.SerializerSettings);
                return await Run_Impl(request, log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(QueryBAN)}_POST HTTP trigger.");
            }
        }

        private static async Task<IActionResult> Run_Impl(
            API.QueryBANRequest request,
            ILogger log,
            ExecutionContext context
        )
        {
            ConfigurationUtils.Initialize(context, log);

            if (!(request?.IsValid ?? false))
            {
                return new BadRequestObjectResult($"Please pass a valid {nameof(API.QueryBANRequest)} object");
            }

            try
            {
                var response = await API.QueryBAN.RunAsync(request);

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
