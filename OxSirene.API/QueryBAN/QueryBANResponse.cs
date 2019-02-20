using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class QueryBANResponse
    {
        [JsonProperty("coordinates")]
        public GeoPoint Coordinates { get; private set; }
        [JsonIgnore]
        public IDictionary<string, object> Properties { get; private set; }
        [JsonProperty("address")]
        public string Address => Properties.TryGetValue("label", out object label) ? label.ToString() : string.Empty;
        [JsonProperty("city")]
        public string City => Properties.TryGetValue("city", out object label) ? label.ToString() : string.Empty;
        [JsonProperty("post_code")]
        public string PostCode => Properties.TryGetValue("postcode", out object label) ? label.ToString() : string.Empty;
        [JsonIgnore]
        public bool IsValid => Properties != null;

        [DebuggerStepThrough]
        public QueryBANResponse(GeoPoint coordinates, IDictionary<string, object> properties, bool strict = true)
        {
            Coordinates = coordinates;
            Properties = properties;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}
