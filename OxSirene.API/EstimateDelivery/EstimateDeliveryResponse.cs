using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class EstimateDeliveryResponse
    {
        /// <summary>
        /// Great circle distance between reseller and delivery point.
        /// </summary>
        [JsonProperty("great_circle")]
        public double GreatCircle { get; private set; }
        /// <summary>
        /// Road distance between reseller and delivery point.
        /// </summary>
        [JsonProperty("distance")]
        public double Distance { get; private set; }
        /// <summary>
        /// Carbon print for a travel by truck over road distance.
        /// </summary>
        [JsonProperty("carbon_print")]
        public double CarbonPrint { get; private set; }
        [JsonIgnore]
        public bool IsValid => GreatCircle >= 0D && Distance >= 0D && CarbonPrint >= 0D;

        [DebuggerStepThrough]
        public EstimateDeliveryResponse(double greatCircle, double distance, double carbonPrint, bool strict = true)
        {
            GreatCircle = greatCircle;
            Distance = distance;
            CarbonPrint = carbonPrint;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}
