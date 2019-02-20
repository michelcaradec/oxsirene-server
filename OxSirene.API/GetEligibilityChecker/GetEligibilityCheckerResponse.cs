using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class GetEligibilityCheckerResponse
    {
        /// <summary>
        /// Market place identifier
        /// </summary>
        [JsonProperty("market_place_id")]
        public string MarketPlaceID { get; private set; }
        /// <summary>
        /// Elligibility check script
        /// </summary>
        [JsonProperty("script")]
        public string Script { get; private set; }
        [JsonIgnore]
        public bool IsValid =>
            !string.IsNullOrEmpty(MarketPlaceID)
            && !string.IsNullOrEmpty(Script);

        [DebuggerStepThrough]
        public GetEligibilityCheckerResponse(string marketPlaceID, string script, bool strict = true)
        {
            MarketPlaceID = marketPlaceID;
            Script = script;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}
