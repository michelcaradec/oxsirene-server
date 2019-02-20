using System;
using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class ScrapProductRequest
    {
        /// <summary>
        /// Web page URL.
        /// </summary>
        [JsonProperty("page")]
        public Uri Page { get; private set; }

        [JsonIgnore]
        public bool IsValid =>
            Page != null
            && Page.IsAbsoluteUri
            && (Page.Scheme.Equals("http", StringComparison.InvariantCultureIgnoreCase)
                || Page.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase)
            );

        [JsonConstructor]
        [DebuggerStepThrough]
        public ScrapProductRequest(string pageUri)
        {
            if (Uri.TryCreate(pageUri, UriKind.Absolute, out Uri page))
            {
                Page = page;
            }
        }

        [DebuggerStepThrough]
        public ScrapProductRequest(Uri page, bool strict = true)
        {
            Page = page;

            if (strict && !IsValid)
            {
                throw new ArgumentException(nameof(page));
            }
        }
    }
}
