using System;
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
    public static class QuerySirene
    {
        private const string UrlString = "https://api.insee.fr";
        public delegate string GetCacheKey(QuerySireneRequest request);

        public delegate Task<string> GetCacheAsync(string key);

        public delegate Task SetCacheAsync(string key, string content);

        #region Local Cache Implementation

        private static string GetLocalCacheKey(QuerySireneRequest request) => $"sirene_{request.Siren}.json";

        private static async Task<string> GetLocalCacheAsync(string key)
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

        private static async Task SetLocalCacheAsync(string key, string content)
        {
            if (Configuration.Instance.UseLocalCache)
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
        private static HttpClient GetClient(string token)
        {
            if (_httpClient == null)
            {
                lock (_lock)
                {
                    WebUtils.SetConnectionLeaseTimeout(new Uri(UrlString));

                    var client = WebUtils.NewHttpClient;

                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    if (!string.IsNullOrEmpty(Configuration.Instance.RequestHeaderFrom)
                        && !client.DefaultRequestHeaders.Contains("From")
                    )
                    {
                        client.DefaultRequestHeaders.Add("From", Configuration.Instance.RequestHeaderFrom);
                    }

                    _httpClient = client;
                }
            }
            else if (!_httpClient.DefaultRequestHeaders.Authorization.Parameter.Equals(token))
            {
                lock (_lock)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
            }

            return _httpClient;
        }

        #endregion

        public static async Task<QuerySireneResponse> RunAsync(QuerySireneRequest request)
        {
            return await RunAsync(
                request,
                GetLocalCacheKey,
                GetLocalCacheAsync,
                SetLocalCacheAsync
            );
        }

        public static async Task<QuerySireneResponse> RunAsync(
            QuerySireneRequest request,
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
                content = await QueryOrganizationsAsync(request.Token, request.Siren, request.Siret);
                await setCache?.Invoke(getCacheKey?.Invoke(request), content);
            }

            var infos = GetOrganizationsInfos(content);

            return new QuerySireneResponse(infos);
        }

        private const int SirenTopCount = 5;

        private static async Task<string> QueryOrganizationsAsync(
            string token,
            string siren,
            string siret
        )
        {
            string query
                = (string.IsNullOrEmpty(siret)
                    ? $"siren:{siren}"
                    : $"siret:{siret}"
                );
            using (var response = await GetClient(token).GetAsync($"{UrlString}/entreprises/sirene/V3/siret?q={query}&nombre={SirenTopCount}"))
            {
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // TODO: handle error
                    // {"fault":{"code":900901,"message":"Invalid Credentials","description":"Access failure for API: /entreprises/sirene/V3, version: V3 status: (900901) - Invalid Credentials. Make sure you have given the correct access token"}}
                    Configuration.Instance.LogInformation(response.ToString());
                    Configuration.Instance.LogInformation(await response.Content.ReadAsStringAsync());
                }
            }

            return null;
        }

        private static IEnumerable<Organization> GetOrganizationsInfos(string content)
        {
            var organizations = new List<Organization>();

            if (!string.IsNullOrEmpty(content))
            {
                var obj = JsonConvert.DeserializeObject<JObject>(content);
                if (obj != null)
                { 
                    foreach (var item in obj["etablissements"].AsJEnumerable())
                    {
                        var properties = new Dictionary<string, object>();
                        properties["siren"] = item["siren"].ToString();
                        properties["siret"] = item["siret"].ToString();
                        properties["name"] = item["uniteLegale"]["denominationUniteLegale"].ToString();
                        properties["naf"] = item["uniteLegale"]["activitePrincipaleUniteLegale"].ToString();
                        properties["category"] = item["uniteLegale"]["categorieEntreprise"].ToString();
                        
                        foreach (var pair in item["adresseEtablissement"].ToObject<Dictionary<string, object>>()
                                            .Where(it => it.Value != null))
                        {
                            properties[pair.Key] = pair.Value;
                        }
                        
                        var organization = new Organization(properties);
                        organizations.Add(organization);
                    }
                }
            }

            return organizations;
        }
    }
}
