using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    /// <summary>
    /// Etablissement.
    /// </summary>
    [JsonObject]
    public class Organization
    {
        /// <summary>
        /// Organization properties.
        /// </summary>
        [JsonIgnore]
        public IDictionary<string, object> Properties { get; private set; }
        /// <summary>
        /// Organization name.
        /// </summary>
        [JsonProperty("name")]
        public string Name => (string)Properties["name"];
        /// <summary>
        /// Organization SIREN.
        /// </summary>
        [JsonProperty("siren")]
        public string Siren => (string)Properties["siren"];
        /// <summary>
        /// Organization SIRET.
        /// </summary>
        /// <returns></returns>
        [JsonProperty("siret")]
        public string Siret => (string)Properties["siret"];

        // FIXME: put SIRENE address fields in config.
        private static readonly IEnumerable<string> _addressProperties
            = new[]
            {
                "numeroVoieEtablissement",
                "typeVoieEtablissement",
                "libelleVoieEtablissement",
                "codePostalEtablissement",
                "libelleCommuneEtablissement"
            };

        /// <summary>
        /// Oganization Full address.
        /// </summary>
        [JsonProperty("address")]
        public string Address
        {
            get
            {
                if (!IsValid)
                {
                    throw new InvalidOperationException();
                }

                return string.Join(
                    " ",
                    _addressProperties.Select(it => Properties.ContainsKey(it) ? Properties[it] : null)
                    .Where(it => it != null)
                );
            }
        }

        [JsonIgnore]
        public bool IsValid => Properties != null;

        [DebuggerStepThrough]
        public Organization(IDictionary<string, object> properties, bool strict = true)
        {
            Properties = properties;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }

    [JsonObject]
    public class QuerySireneResponse
    {
        /// <summary>
        /// List of company's organizations.
        /// </summary>
        [JsonProperty("organizations")]
        public IEnumerable<Organization> Organizations { get; private set; }
        [JsonIgnore]
        public bool IsValid => Organizations != null;

        [DebuggerStepThrough]
        public QuerySireneResponse(IEnumerable<Organization> organizations, bool strict = true)
        {
            Organizations = organizations;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}
