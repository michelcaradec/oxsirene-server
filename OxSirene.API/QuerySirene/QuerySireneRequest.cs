using System;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class QuerySireneRequest
    {
        /// <summary>
        /// SIRENE API access token.
        /// </summary>
        [JsonIgnore]
        public string Token { get; private set; }
        [JsonProperty("siren")]
        /// <summary>
        /// SIREN.
        /// </summary>
        public string Siren { get; private set; }
        /// <summary>
        /// SIRET.
        /// </summary>
        [JsonProperty("siret")]
        public string Siret { get; private set; }
        [JsonIgnore]
        public bool IsValid => !string.IsNullOrEmpty(Token) && CheckValidity(Siren, Siret);

        [JsonConstructor]
        [DebuggerStepThrough]
        public QuerySireneRequest(string siren, string siret)
        {
            Siren = siren;
            Siret = siret;
        }

        [DebuggerStepThrough]
        public QuerySireneRequest(string token, string siren, string siret, bool strict = true)
            : this(siren, siret)
        {
            Token = token;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }

        [DebuggerStepThrough]
        public static QuerySireneRequest FromSiren(string token, string siren) => new QuerySireneRequest(token, siren, null);

        [DebuggerStepThrough]
        public static QuerySireneRequest FromSiret(string token, string siret) => new QuerySireneRequest(token, null, siret);

        [DebuggerStepThrough]
        public static QuerySireneRequest FromCode(string token, string code)
        {
            if (SireneUtils.IsSiren(code))
            {
                return FromSiren(token, code);
            }
            else if (SireneUtils.IsSiret(code))
            {
                return FromSiret(token, code);
            }
            else
            {
                return new QuerySireneRequest(null, null);
            }
        }

        public static QuerySireneRequest FromCode(string code)
        {
            if (SireneUtils.IsSiren(code))
            {
                return new QuerySireneRequest(code, null);
            }
            else if (SireneUtils.IsSiret(code))
            {
                return new QuerySireneRequest(null, code);
            }
            else
            {
                return new QuerySireneRequest(null, null);
            }
        }

        public static bool CheckValidity(string siren, string siret) => !string.IsNullOrEmpty(siren) || !string.IsNullOrEmpty(siret);
    }
}
