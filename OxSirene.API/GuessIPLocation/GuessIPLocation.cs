using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OxSirene.API
{
    public static class GuessIPLocation
    {
        private const string UrlString = "https://api.ipgeolocation.io";

        public delegate string GetCacheKey(GuessIPLocationRequest request);

        public delegate Task<string> GetCacheAsync(string key);

        public delegate Task SetCacheAsync(string key, string content);

        #region Local Cache Implementation

        public static string GetLocalCacheKey(GuessIPLocationRequest request) => $"{request.IP}.json";

        public static async Task<string> GetLocalCacheAsync(string key)
        {
            if (key != null)
            {
                string fileName = LocalCacheUtils.GetFullPath(key);
                if (Configuration.Instance.UseLocalCache && File.Exists(fileName))
                {
                    using (var file = File.OpenText(fileName))
                    {
                        return await file.ReadToEndAsync();
                    }
                }
            }

            return null;
        }

        public static async Task SetLocalCacheAsync(string key, string content)
        {
            if (key != null && Configuration.Instance.UseLocalCache)
            {
                string fileName = LocalCacheUtils.GetFullPath(key);
                using (var file = File.CreateText(fileName))
                {
                    await file.WriteLineAsync(content);
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

        public static async Task<GuessIPLocationResponse> RunAsync(GuessIPLocationRequest request)
        {
            return await RunAsync(
                request,
                GetLocalCacheKey,
                GetLocalCacheAsync,
                SetLocalCacheAsync
            );
        }

        public static async Task<GuessIPLocationResponse> RunAsync(
            GuessIPLocationRequest request,
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

            string content = await getCache?.Invoke(getCacheKey?.Invoke(request));
            if (string.IsNullOrEmpty(content))
            {
                content = await QueryIPGeolocationAsync(request.IP);
                await setCache?.Invoke(getCacheKey?.Invoke(request), content);
            }
            
            return new GuessIPLocationResponse(GetLocation(content), false);
        }

        private static async Task<string> QueryIPGeolocationAsync(IPAddress ip)
        {
            using (var response = await Client.GetAsync($"{UrlString}/ipgeo?apiKey={Configuration.Instance["api:geo_ip:ipgeolocation_apikey"]}&ip={ip}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }

            return null;
        }

        private static GeoPoint GetLocation(string content)
        {
            if (!string.IsNullOrEmpty(content))
            {
                var obj = JsonConvert.DeserializeObject<JObject>(content);
                if (obj != null)
                {
                    return new GeoPoint(
                        double.Parse(obj["longitude"].ToString()),
                        double.Parse(obj["latitude"].ToString())
                    );
                }
            }

            return null;
        }
    }
}
