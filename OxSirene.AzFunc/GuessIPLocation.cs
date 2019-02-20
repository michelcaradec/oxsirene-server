using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace OxSirene.AzFunc
{
    public static class GuessIPLocation
    {
        [FunctionName("GuessIPLocation_GET")]
        public static async Task<IActionResult> Run_GET(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/location/{ip?}")] HttpRequestMessage req,
            string ip,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(GuessIPLocation)}_GET HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                if (string.IsNullOrEmpty(ip))
                {
                    ip = HttpUtils.GetIpFromRequestHeaders(req);
                    log.LogInformation($"IP taken from request header: {ip}");

                    // if (string.IsNullOrEmpty(ip))
                    // {
                    //     return new StatusCodeResult(StatusCodes.Status400BadRequest);
                    // }
                }

                return await Run_Impl(ip, log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(GuessIPLocation)}_GET HTTP trigger.");
            }
        }

        [FunctionName("GuessIPLocation_POST")]
        public static async Task<IActionResult> Run_POST(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/location")] HttpRequestMessage req,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(GuessIPLocation)}_POST HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                string requestBody = await req.Content.ReadAsStringAsync();
                log.LogDebug(requestBody);

                var body = JsonConvert.DeserializeObject<JObject>(requestBody);
                return await Run_Impl(body["ip"]?.Value<string>(), log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(GuessIPLocation)}_POST HTTP trigger.");
            }
        }

        private static async Task<IActionResult> Run_Impl(
            string ipString,
            ILogger log,
            ExecutionContext context
        )
        {
            ConfigurationUtils.Initialize(context, log);

            if (!IPAddress.TryParse(ipString, out IPAddress ip))
            {
                return new BadRequestObjectResult($"Please pass a valid {nameof(API.GuessIPLocationRequest)} object");
            }
            
            var request = new API.GuessIPLocationRequest(ip);

            try
            {
                var response = await API.GuessIPLocation.RunAsync(request);

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
