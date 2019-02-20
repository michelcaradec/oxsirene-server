using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class ScrapSellerRequest
    {
        /// <summary>
        /// Market place identifier.
        /// </summary>
        [JsonProperty("market_place_id")]
        public string MarketPlaceID { get; private set; }
        /// <summary>
        /// Seller identifier.
        /// </summary>
        [JsonProperty("seller_id")]
        public string SellerID { get; private set; }
        [JsonIgnore]
        public bool IsValid => !string.IsNullOrEmpty(MarketPlaceID) && !string.IsNullOrEmpty(SellerID);

        [JsonConstructor]
        [DebuggerStepThrough]
        public ScrapSellerRequest()
        {
        }

        [DebuggerStepThrough]
        public ScrapSellerRequest(string marketPlaceID, string sellerID, bool strict = true)
        {
            MarketPlaceID = marketPlaceID;
            SellerID = sellerID;

            if (strict && !IsValid)
            {
                throw new ArgumentException(nameof(sellerID));
            }
        }
    }
}
