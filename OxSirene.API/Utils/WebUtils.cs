using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.Net;

namespace OxSirene.API
{
    internal static class WebUtils
    {
        #region Shared HTTP client

        public static void SetConnectionLeaseTimeout(Uri url, TimeSpan? connectionLeaseTimeout = null)
        {
            // https://blogs.msdn.microsoft.com/shacorn/2016/10/21/best-practices-for-using-httpclient-on-services/
            ServicePointManager.FindServicePoint(url).ConnectionLeaseTimeout
                = (int)(connectionLeaseTimeout.HasValue
                        ? connectionLeaseTimeout.Value
                        : Configuration.Instance.ConnectionLeaseTimeout
                ).TotalMilliseconds;
        }

        public static HttpClient NewHttpClient
        {
            get
            {
                var httpClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });

                // Set Keep-Alive on.
                httpClient.DefaultRequestHeaders.Connection.Clear();
                httpClient.DefaultRequestHeaders.ConnectionClose = false;
                httpClient.DefaultRequestHeaders.Connection.Add("Keep-Alive");

                return httpClient;
            }
        }

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
                        _httpClient = WebUtils.NewHttpClient;

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

        public static string GetWebPage(Uri uri)
        {
            var task = GetWebPageAsync(uri);
            Task.WaitAll(task);
            return task.Result;
        }

        public static async Task<string> GetWebPageAsync(Uri uri)
        {
            using (var response = await Client.GetStreamAsync(uri))
            {
                using (var reader = new StreamReader(response))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
    }
}