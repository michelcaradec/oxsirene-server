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
    public static class EstimateDelivery
    {
        [FunctionName("EstimateDelivery_GET")]
        public static async Task<IActionResult> Run_GET(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/delivery/estimate/{from_lon}/{from_lat}/{to_lon}/{to_lat}")] HttpRequest req,
            double from_lon,
            double from_lat,
            double to_lon,
            double to_lat,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(EstimateDelivery)}_GET HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                var request = new API.EstimateDeliveryRequest(
                    new API.GeoPoint(from_lon, from_lat),
                    new API.GeoPoint(to_lon, to_lat)
                );
                return await Run_Impl(request, log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(EstimateDelivery)}_GET HTTP trigger.");
            }
        }

        [FunctionName("EstimateDelivery_POST")]
        public static async Task<IActionResult> Run_POST(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/delivery/estimate")] HttpRequestMessage req,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(EstimateDelivery)}_POST HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                string requestBody = await req.Content.ReadAsStringAsync();
                log.LogDebug(requestBody);
                var request = JsonConvert.DeserializeObject<API.EstimateDeliveryRequest>(requestBody, JsonUtils.SerializerSettings);
                return await Run_Impl(request, log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(EstimateDelivery)}_POST HTTP trigger.");
            }
        }

        private static async Task<IActionResult> Run_Impl(
            API.EstimateDeliveryRequest request,
            ILogger log,
            ExecutionContext context
        )
        {
            ConfigurationUtils.Initialize(context, log);

            if (!(request?.IsValid ?? false))
            {
                return new BadRequestObjectResult($"Please pass a valid {nameof(API.EstimateDeliveryRequest)} object");
            }

            try
            {
                var response = await API.EstimateDelivery.RunAsync(request);

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
