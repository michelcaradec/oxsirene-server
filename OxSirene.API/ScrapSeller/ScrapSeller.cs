using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace OxSirene.API
{
    public static class ScrapSeller
    {
        public delegate string GetCacheKey(ScrapSellerRequest request);

        public delegate Task<string> GetCacheAsync(string key);

        public delegate Task SetCacheAsync(string key, string content);

        #region Local Cache Implementation

        private static string GetLocalCacheKey(ScrapSellerRequest request) =>
            $"seller_{HashUtils.ToMD5(request.SellerID)}.html";

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

        public static async Task<ScrapSellerResponse> RunAsync(ScrapSellerRequest request)
        {
            return await RunAsync(
                request,
                GetLocalCacheKey,
                GetLocalCacheAsync,
                SetLocalCacheAsync
            );
        }

        public static async Task<ScrapSellerResponse> RunAsync(
            ScrapSellerRequest request,
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

            var scrapper = ScrapSellerFactory.CreateInstance(request.MarketPlaceID);
            if (scrapper == null)
            {
                throw new NotSupportedException(request.MarketPlaceID);
            }

            string content = await getCache?.Invoke(getCacheKey?.Invoke(request));
            if (string.IsNullOrEmpty(content))
            {
                content = await WebUtils.GetWebPageAsync(scrapper.GetPageUri(request.SellerID));
                await setCache?.Invoke(getCacheKey?.Invoke(request), content);
            }

            var properties = scrapper.GetProperties(content);

            return new ScrapSellerResponse(request.SellerID, properties);
        }
    }
}
