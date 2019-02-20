using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class GuessIPLocationResponse
    {
        [JsonProperty("coordinates")]
        public GeoPoint Coordinates { get; private set; }
        [JsonIgnore]
        public bool IsValid => Coordinates != null;

        [DebuggerStepThrough]
        public GuessIPLocationResponse(GeoPoint coordinates, bool strict = true)
        {
            Coordinates = coordinates;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}
