using System;
using Newtonsoft.Json;

namespace OxSirene.API
{
    internal static class JsonUtils
    {
        public static JsonSerializerSettings SerializerSettings =>
            new JsonSerializerSettings()
            {
                Error = delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                {
                    args.ErrorContext.Handled = true;
                }
            };
    }
}