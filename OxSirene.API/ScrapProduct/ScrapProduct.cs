using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace OxSirene.API
{
    public static class ScrapProduct
    {
        public delegate string GetCacheKey(ScrapProductRequest request);

        public delegate Task<string> GetCacheAsync(string key);

        public delegate Task SetCacheAsync(string key, string content);

        #region Local Cache Implementation

        private static string GetLocalCacheKey(ScrapProductRequest request) =>
            $"product_{HashUtils.ToMD5(request.Page.AbsoluteUri)}.html";

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

        public static async Task<ScrapProductResponse> RunAsync(ScrapProductRequest request)
        {
            return await RunAsync(
                request,
                GetLocalCacheKey,
                GetLocalCacheAsync,
                SetLocalCacheAsync
            );
        }

        public static async Task<ScrapProductResponse> RunAsync(
            ScrapProductRequest request,
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

            var scraper = ScrapProductFactory.CreateInstance(request.Page);
            if (scraper == null)
            {
                throw new NotSupportedException(request.Page.Host);
            }

            string content = await getCache?.Invoke(getCacheKey?.Invoke(request));
            if (string.IsNullOrEmpty(content))
            {
                content = await WebUtils.GetWebPageAsync(request.Page);
                await setCache?.Invoke(getCacheKey?.Invoke(request), content);
            }

            string productName = scraper.GetProductName(content);
            var sellerIDs = scraper.GetSellers(content).Distinct().ToList();

            return new ScrapProductResponse(scraper.MarketPlaceID, productName, sellerIDs, false);
        }      
    }
}
