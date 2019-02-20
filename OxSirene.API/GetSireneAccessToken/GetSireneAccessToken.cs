using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace OxSirene.API
{
    public static class GetSireneAccessToken
    {
        private const string UrlString = "https://api.insee.fr";

        /// <remarks>
        /// INSEE recommends its users to renew the token once a day.
        /// </remarks>
        private static readonly TimeSpan AgeMax = TimeSpan.FromDays(1);

        public delegate string GetCacheKey(GetSireneAccessTokenRequest request);

        public delegate Task<GetSireneAccessTokenResponse> GetCacheAsync(string key);

        public delegate Task SetCacheAsync(string key, GetSireneAccessTokenResponse token);

        #region Local Cache Implementation

        public static string GetLocalCacheKey(GetSireneAccessTokenRequest request) => "sirene_token.json";

        private static async Task<GetSireneAccessTokenResponse> GetLocalCacheAsync(string key)
        {
            if (key != null)
            {
                string fileName = LocalCacheUtils.GetFullPath(key);
                if (Configuration.Instance.UseLocalCache && File.Exists(fileName))
                {
                    using (var file = File.OpenText(fileName))
                    {
                        string json = await file.ReadToEndAsync();
                        return JsonConvert.DeserializeObject<GetSireneAccessTokenResponse>(json, JsonUtils.SerializerSettings);
                    }
                }
            }

            return null;
        }

        private static async Task SetLocalCacheAsync(string key, GetSireneAccessTokenResponse token)
        {
            if (Configuration.Instance.UseLocalCache)
            {
                string fileName = LocalCacheUtils.GetFullPath("sirene_token.json");
                using (var file = File.CreateText(fileName))
                {
                    await file.WriteLineAsync(JsonConvert.SerializeObject(token));
                }
            }
        }

        #endregion
        #region Shared HTTP client
        
        private static object _lock = new object();
        private static HttpClient _httpClient = null;

        /// <summary>
        /// Shared HTTP client.
        /// </summary>
        /// <remarks>
        /// Avoid socket exhaustion problem.
        /// </remarks>
        private static HttpClient Client
        {
            get
            {
                if (_httpClient == null)
                {
                    lock (_lock)
                    {
                        WebUtils.SetConnectionLeaseTimeout(new Uri(UrlString));

                        _httpClient = WebUtils.NewHttpClient;

                        _httpClient.DefaultRequestHeaders.Accept.Clear();
                        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        _httpClient.DefaultRequestHeaders.Authorization
                            = new AuthenticationHeaderValue("Basic", Configuration.Instance["api:get_sirene_access_token:sirene_client_credentials"]);
                        if (!string.IsNullOrEmpty(Configuration.Instance.RequestHeaderFrom)
                            && !_httpClient.DefaultRequestHeaders.Contains("From")
                        )
                        {
                            _httpClient.DefaultRequestHeaders.Add("From", Configuration.Instance.RequestHeaderFrom);
                        }
                    }
                }

                return _httpClient;
            }
        }

        #endregion

        public static async Task<GetSireneAccessTokenResponse> RunAsync(GetSireneAccessTokenRequest request)
        {
            return await RunAsync(
                request,
                GetLocalCacheKey,
                GetLocalCacheAsync,
                SetLocalCacheAsync
            );
        }

        public static async Task<GetSireneAccessTokenResponse> RunAsync(
            GetSireneAccessTokenRequest request,
            GetCacheKey getCacheKey,
            GetCacheAsync getCache,
            SetCacheAsync setCache
        )
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }
            if (!request.IsValid)
            {
                throw new ArgumentException(nameof(request));
            }

            API.GetSireneAccessTokenResponse respAccessToken;

            if (request.IsCheckCache)
            {
                respAccessToken = await getCache?.Invoke(getCacheKey?.Invoke(request));
                if (respAccessToken != null)
                {
                    Configuration.Instance.LogInformation($"Sirene access token loaded from cache: {JsonConvert.SerializeObject(respAccessToken)}");
                }
                if (respAccessToken?.Age < AgeMax && !(respAccessToken?.IsExpired ?? true))
                {
                    Configuration.Instance.LogInformation("Sirene access token recycled.");
                    return respAccessToken;
                }
                Configuration.Instance.LogInformation("Sirene access token out of date.");
            }

            Configuration.Instance.LogInformation("Request new Sirene access token.");

            respAccessToken = await GetSireneAccessTokenAsync();
            await setCache?.Invoke(getCacheKey?.Invoke(request), respAccessToken);

            return respAccessToken;            
        }

        private static async Task<GetSireneAccessTokenResponse> GetSireneAccessTokenAsync()
        {
            using (var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("grant_type", "client_credentials") }))
            {
                using (var message = new HttpRequestMessage(HttpMethod.Post, $"{UrlString}/token") { Content = content })
                {
                    using (var response = await Client.SendAsync(message))
                    {
                        GetSireneAccessTokenResponse respAccessToken = null;
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();
                            GetSireneAccessTokenResponse.TryParse(json, out respAccessToken);
                        }
                        else
                        {
                            Configuration.Instance.LogError($"Web request error '{UrlString}': {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                        }

                        return respAccessToken;
                    }
                }
            }
        }
    }
}
