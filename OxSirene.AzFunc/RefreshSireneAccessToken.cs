using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OxSirene.AzFunc
{
    public static class RefreshSireneAccessToken
    {
        [FunctionName("RefreshSireneAccessToken")]
        public static async Task Run(
            //[TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer,
            // INSEE recommends its users to renew the token once a day.
            // Every day at 06:00:00
            [TimerTrigger("0 0 6 * * *", RunOnStartup = true)] TimerInfo myTimer,
            ILogger log,
            ExecutionContext context
        )
        {
            log.LogInformation($"Start {nameof(RefreshSireneAccessToken)} Timer trigger.");

            try
            {
                await Run_Impl(false, log, context);
            }
            finally
            {
                log.LogInformation($"End {nameof(RefreshSireneAccessToken)} Timer trigger (next execution at {myTimer.Schedule.GetNextOccurrence(DateTime.UtcNow)}).");
            }
        }

        private static async Task Run_Impl(
            bool checkCache,
            ILogger log,
            ExecutionContext context
        )
        {
            ConfigurationUtils.Initialize(context, log);

            var responseToken = await API.GetSireneAccessToken.RunAsync(
                new API.GetSireneAccessTokenRequest(checkCache),
                API.GetSireneAccessToken.GetLocalCacheKey,
                async (key) =>
                {
                    string json = await StorageUtils.ReadContentAsync($"{StorageUtils.CacheContainerName}/{key}");

                    return json == null ? null : JsonConvert.DeserializeObject<API.GetSireneAccessTokenResponse>(json, JsonUtils.SerializerSettings);
                },
                async (key, token) => await StorageUtils.WriteContentAsync($"{StorageUtils.CacheContainerName}/{key}", token == null ? null : JsonConvert.SerializeObject(token))
            );
            if (!responseToken.IsValid)
            {
                log.LogError("Failed to get Sirene access token");
            }
        }
    }
}
