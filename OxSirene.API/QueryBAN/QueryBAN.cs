using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OxSirene.API
{
    public static class QueryBAN
    {
        private const string UrlString = "https://api-adresse.data.gouv.fr";

        public delegate string GetCacheKey(QueryBANRequest request);

        public delegate Task<string> GetCacheAsync(string key);

        public delegate Task SetCacheAsync(string key, string content);

        #region Local Cache Implementation

        public static string GetLocalCacheKey(QueryBANRequest request) =>
            $"ban_{HashUtils.ToMD5(request.Coordinates?.ToString() ?? request.Address)}.json";

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

        public static async Task<QueryBANResponse> RunAsync(QueryBANRequest request)
        {
            return await RunAsync(
                request,
                GetLocalCacheKey,
                GetLocalCacheAsync,
                SetLocalCacheAsync
            );
        }

        public static async Task<QueryBANResponse> RunAsync(
            QueryBANRequest request,
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
                content
                    = request.Coordinates == null
                    ? await LookupAddressAsync(request.Address)
                    : await LookupLocationAsync(request.Coordinates);
                await setCache?.Invoke(getCacheKey?.Invoke(request), content);
            }

            return GetAddressInfos(content);
        }

        private const int BANTopCount = 1;

        private static async Task<string> LookupAddressAsync(string address)
        {
            using (var response = await Client.GetAsync($"{UrlString}/search/?q={address}&limit={BANTopCount}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }

            return null;
        }

        private static async Task<string> LookupLocationAsync(GeoPoint location)
        {
            using (var response = await Client.GetAsync($"{UrlString}/reverse/?lon={location.Lon}&lat={location.Lat}&type=street&limit={BANTopCount}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }

            return null;
        }

        private static QueryBANResponse GetAddressInfos(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            var obj = JsonConvert.DeserializeObject<JObject>(content);
            if (obj == null)
            {
                return null;
            }
            
            var feature = obj["features"]?.FirstOrDefault();
            if (feature == null)
            {
                return null;
            }
            
            var coordinates = feature["geometry"]["coordinates"];

            return new QueryBANResponse(
                new GeoPoint(
                    coordinates[0].Value<double>(),
                    coordinates[1].Value<double>()
                ),
                feature["properties"].ToObject<Dictionary<string, object>>()
            );
        }
    }
}
