using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class GetSireneAccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string Token { get; private set; }
        [JsonProperty("scope")]
        public string Scope { get; private set; }
        [JsonProperty("token_type")]
        public string Type { get; private set; }
        [JsonProperty("expires_in")]
        public int ExpiresInSeconds { get; set; }
        [JsonIgnore]
        public TimeSpan ExpiresIn => TimeSpan.FromSeconds(ExpiresInSeconds);
        [JsonIgnore]
        public TimeSpan ExpiresInFromNow => DateOfRequest.Add(ExpiresIn) - DateTimeOffset.UtcNow;
        [JsonIgnore]
        public TimeSpan Age => DateTimeOffset.UtcNow - DateOfRequest;
        [JsonProperty("date_of_request")]
        public DateTimeOffset DateOfRequest { get; private set; }
        [JsonIgnore]
        public bool IsExpired => ExpiresInFromNow <= TimeSpan.Zero;
        [JsonIgnore]
        public bool IsValid => !string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(Type);

        [DebuggerStepThrough]
        public GetSireneAccessTokenResponse(string token, string scope, string type, int expiresInSeconds, bool strict = true)
        {
            DateOfRequest = DateTimeOffset.UtcNow;
            Token = token;
            Scope = scope;
            Type = type;
            ExpiresInSeconds = expiresInSeconds;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }

        public static bool TryParse(string json, out GetSireneAccessTokenResponse response)
        {
            try
            {
                response = JsonConvert.DeserializeObject<GetSireneAccessTokenResponse>(json);
                response.DateOfRequest = DateTimeOffset.UtcNow;
                return true;
            }
            catch
            {
                response = null;
                return false;
            }
        }
    }
}
