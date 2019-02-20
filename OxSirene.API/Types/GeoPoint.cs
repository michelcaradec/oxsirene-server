using System;
using System.Diagnostics;
using System.Globalization;
using Newtonsoft.Json;

namespace OxSirene.API
{
    [JsonObject]
    public class GeoPoint
    {
        private const char Separator = ';';

        [JsonProperty("lon")]
        public double Lon { get; private set; }
        [JsonProperty("lat")]
        public double Lat { get; private set; }

        [DebuggerStepThrough]
        public GeoPoint(double lon, double lat)
        {
            Lon = lon;
            Lat = lat;
        }

        public static bool TryParse(string locationString, out GeoPoint location)
        {
            location = null;

            if (string.IsNullOrEmpty(locationString))
            {
                return false;
            }

            var parts = locationString.Split(Separator, StringSplitOptions.None);
            if (parts.Length != 2)
            {
                return false;
            }

            if (double.TryParse(parts[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double lon)
                && double.TryParse(parts[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double lat)
            )
            {
                location = new GeoPoint(lon, lat);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static GeoPoint Parse(string locationString)
        {
            if (TryParse(locationString, out GeoPoint location))
            {
                return location;
            }
            else
            {
                throw new ArgumentException(locationString, nameof(locationString));
            }
        }

        // /// <summary>
        // /// Tuple deconstruction.
        // /// </summary>
        // /// <example>
        // /// <c>var (lon, lat) = new GeoPoint(-1.6736461, 48.0930176);</c>
        // /// </example>
        // public void Deconstruct(out double lon, out double lat) { lon = Lon; lat = Lat; }

        public override string ToString() => $"{Lon}{Separator}{Lat}";
    }
}