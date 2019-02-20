using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class EstimateDeliveryRequest
    {
        /// <summary>
        /// Starting geo-location.
        /// </summary>
        [JsonProperty("from")]
        public GeoPoint From { get; private set; }
        /// <summary>
        /// Ending geo-location
        /// </summary>
        [JsonProperty("to")]
        public GeoPoint To { get; private set; }
        [JsonIgnore]
        public bool IsValid => From != null && To != null;

        [DebuggerStepThrough]
        public EstimateDeliveryRequest(GeoPoint from, GeoPoint to, bool strict = true)
        {
            From = from;
            To = to;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}
