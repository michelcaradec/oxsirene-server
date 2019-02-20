using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class QueryBANRequest
    {
        /// <summary>
        /// Address.
        /// </summary>
        [JsonProperty("address")]
        public string Address { get; private set; }
        /// <summary>
        /// Geo-location.
        /// </summary>
        [JsonProperty("coordinates")]
        public GeoPoint Coordinates { get; private set; }

        public bool IsValid => Coordinates != null || !string.IsNullOrEmpty(Address);

        [JsonConstructor]
        public QueryBANRequest()
        {
        }

        [DebuggerStepThrough]
        public QueryBANRequest(string address, bool strict = true)
        {
            Address = address;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }

        [DebuggerStepThrough]
        public QueryBANRequest(GeoPoint coordinates, bool strict = true)
        {
            Coordinates = coordinates;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }

        [DebuggerStepThrough]
        public QueryBANRequest(string address, GeoPoint coordinates, bool strict = true)
        {
            Address = address;
            Coordinates = coordinates;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}
