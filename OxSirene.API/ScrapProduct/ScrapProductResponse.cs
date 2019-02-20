using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class ScrapProductResponse
    {
        /// <summary>
        /// Market place identifier.
        /// </summary>
        [JsonProperty("market_place_id")]
        public string MarketPlaceID { get; private set; }
        /// <summary>
        /// Product name.
        /// </summary>
        [JsonProperty("product_name")]
        public string ProductName { get; private set; }
        /// <summary>
        /// Sellers identifiers.
        /// </summary>
        [JsonProperty("seller_ids")]
        public IEnumerable<string> SellerIDs { get; private set; }
        [JsonIgnore]
        public bool IsValid =>
            !string.IsNullOrEmpty(MarketPlaceID)
            && !string.IsNullOrEmpty(ProductName)
            && SellerIDs != null;

        [DebuggerStepThrough]
        public ScrapProductResponse(string marketPlaceID, string productName, IEnumerable<string> sellerIDs, bool strict = true)
        {
            MarketPlaceID = marketPlaceID;
            ProductName = productName;
            SellerIDs = sellerIDs;

            if (strict && !IsValid)
            {
                throw new ArgumentException(nameof(sellerIDs));
            }
        }
    }
}
