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
    public static class QuerySirene
    {
        [FunctionName("QuerySirene_GET")]
        public static async Task<IActionResult> Run_GET(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/sirene/{siren}")] HttpRequestMessage req,
            string siren,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(QuerySirene)}_GET HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                log.LogDebug(siren);
                var request = API.QuerySireneRequest.FromCode(siren);

                return await Run_Impl(request, log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(QuerySirene)}_GET HTTP trigger.");
            }
        }

        [FunctionName("QuerySirene_POST")]
        public static async Task<IActionResult> Run_POST(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/sirene")] HttpRequestMessage req,
            ILogger log,
            ExecutionContext context
        )
        {
            string correlationID = HttpUtils.GetCorrelationID(req);
            log.LogInformation($"Start {nameof(QuerySirene)}_POST HTTP trigger.");
            log.LogInformation($"CorrelationID: {correlationID}.");

            try
            {
                string requestBody = await req.Content.ReadAsStringAsync();
                log.LogDebug(requestBody);
                var request = JsonConvert.DeserializeObject<API.QuerySireneRequest>(requestBody, JsonUtils.SerializerSettings);

                return await Run_Impl(request, log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(QuerySirene)}_POST HTTP trigger.");
            }
        }

        private static async Task<IActionResult> Run_Impl(
            API.QuerySireneRequest request,
            ILogger log,
            ExecutionContext context
        )
        {
            ConfigurationUtils.Initialize(context, log);

            if (!API.QuerySireneRequest.CheckValidity(request?.Siren, request?.Siret))
            {
                return new BadRequestObjectResult($"Please pass a valid {nameof(API.QuerySireneRequest)} object");
            }

            #region Get Sirene Access Token
            {
                // Last chance to get a valid Sirene access token.
                // Stored token should always be valid if `RefreshSireneAccessToken` timer trigger is properly executed.
                var responseToken = await API.GetSireneAccessToken.RunAsync(
                    new API.GetSireneAccessTokenRequest(),
                    API.GetSireneAccessToken.GetLocalCacheKey,
                    async (key) =>
                    {
                        string json = await StorageUtils.ReadContentAsync($"{StorageUtils.CacheContainerName}/{key}");
                        return json == null ? null : JsonConvert.DeserializeObject<API.GetSireneAccessTokenResponse>(json, JsonUtils.SerializerSettings);
                    },
                    async (key, token) => await StorageUtils.WriteContentAsync($"{StorageUtils.CacheContainerName}/{key}", JsonConvert.SerializeObject(token))
                );
                if (!responseToken.IsValid)
                {
                    log.LogError("Failed to get Sirene access token");
                    return new StatusCodeResult(StatusCodes.Status500InternalServerError);
                }

                request = new API.QuerySireneRequest(responseToken.Token, request.Siren, request.Siret);
            }
            #endregion

            try
            {
                var response = await API.QuerySirene.RunAsync(request);

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
