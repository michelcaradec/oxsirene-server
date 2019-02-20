using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace OxSirene.AzFunc
{
    internal static class HttpUtils
    {
        private static string HttpHeaderForwardedFor = "X-Forwarded-For";
        private const string HttpHeaderRequestID = "X-Request-ID";

        // https://stackoverflow.com/questions/37582553/how-to-get-client-ip-address-in-azure-functions-c
        public static string GetIpFromRequestHeaders(HttpRequestMessage request)
        {
            if (request.Headers.TryGetValues(HttpHeaderForwardedFor, out var values))
            {
                return values.FirstOrDefault().Split(new char[] { ',' }).FirstOrDefault().Split(new char[] { ':' }).FirstOrDefault();
            }

            return string.Empty;
        }

        public static string GetCorrelationID(HttpRequestMessage request)
        {
            if (request.Headers.TryGetValues(HttpHeaderRequestID, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }

        public static string GetCorrelationID(HttpRequest request)
        {
            if (request.Headers.TryGetValue(HttpHeaderRequestID, out var values))
            {
                return values.FirstOrDefault();
            }

            return null;
        }
    }
}