using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class ScrapSellerResponse
    {
        public const string CommercialIDPropertyName = "_siren";
        public const string CommercialNamePropertyName = "_name";

        /// <summary>
        /// Seller identifier.
        /// </summary>
        [JsonProperty("seller_id")]
        public string SellerID { get; private set; }
        /// <summary>
        /// Seller name.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("name")]
        public string Name => Properties.TryGetValue(CommercialNamePropertyName, out string name) ? name : null;
        /// <summary>
        /// Seller SIREN.
        /// </summary>
        [JsonProperty("siren")]
        public string Siren
        {
            get
            {
                if (Properties.TryGetValue(CommercialIDPropertyName, out string siren))
                {
                    siren = Regex.Replace(siren, "[^\\d]", string.Empty);
                    if (siren.Length >= SireneUtils.SirenLength)
                    {
                        return siren.Substring(0, SireneUtils.SirenLength);
                    }
                }

                return null;
            }
        }
        /// <summary>
        /// Seller SIRET.
        /// </summary>
        [JsonProperty("siret")]
        public string Siret
        {
            get
            {
                if (Properties.TryGetValue(CommercialIDPropertyName, out string siren))
                {
                    siren = Regex.Replace(siren, "[^\\d]", string.Empty);
                    if (siren.Length >= SireneUtils.SiretLength)
                    {
                        return siren.Substring(0, SireneUtils.SiretLength);
                    }
                }

                return null;
            }
        }
        /// <summary>
        /// Seller's details.
        /// </summary>
        [JsonProperty("properties")]
        public IDictionary<string, string> Properties { get; private set; }
        [JsonIgnore]
        public bool IsValid => !string.IsNullOrEmpty(SellerID);

        [DebuggerStepThrough]
        public ScrapSellerResponse(string sellerID, IDictionary<string, string> properties = null, bool strict = true)
        {
            SellerID = sellerID;
            Properties = properties ?? new Dictionary<string, string>();

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}
