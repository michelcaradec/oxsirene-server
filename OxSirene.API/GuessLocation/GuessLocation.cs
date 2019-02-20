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
    /// <summary>
    /// High level API for location informations interrogation.
    /// </summary>
    public static class GuessLocation
    {
        public static async Task<GuessLocationResponse> RunAsync(GuessLocationRequest request)
        {
            return await RunAsync(
                request,
                GuessIPLocation.GetLocalCacheKey,
                GuessIPLocation.GetLocalCacheAsync,
                GuessIPLocation.SetLocalCacheAsync,
                QueryBAN.GetLocalCacheKey,
                QueryBAN.GetLocalCacheAsync,
                QueryBAN.SetLocalCacheAsync
            );
        }

        public static async Task<GuessLocationResponse> RunAsync(
            GuessLocationRequest request,
            GuessIPLocation.GetCacheKey getCacheKeyIP,
            GuessIPLocation.GetCacheAsync getCacheIP,
            GuessIPLocation.SetCacheAsync setCacheIP,
            QueryBAN.GetCacheKey getCacheKeyBAN,
            QueryBAN.GetCacheAsync getCacheBAN,
            QueryBAN.SetCacheAsync setCacheBAN
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

            GuessLocationRequest request2;
            if (request.Type == LocationType.IP)
            {
                var responseIP = await GuessIPLocation.RunAsync(
                    new GuessIPLocationRequest(request.IP),
                    getCacheKeyIP,
                    getCacheIP,
                    setCacheIP
                );
                if (responseIP.IsValid)
                {
                    request2 = new GuessLocationRequest(responseIP.Coordinates);
                }
                else
                {
                    return GuessLocationResponse.Null;
                }
            }
            else
            {
                request2 = request;
            }

            var responseBAN = await QueryBAN.RunAsync(
                new QueryBANRequest(request2.Address, request2.Coordinates),
                getCacheKeyBAN,
                getCacheBAN,
                setCacheBAN
            );
            if (responseBAN.IsValid)
            {
                return new GuessLocationResponse(responseBAN.Coordinates, responseBAN.Address);
            }
            else
            {
                return GuessLocationResponse.Null;
            }
        }
    }
}