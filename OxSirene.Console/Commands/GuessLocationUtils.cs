using System;
using System.Net;

namespace OxSirene.Console
{
    internal static class GuessLocationUtils
    {
        private const int AddressLengthMin = 3;

        public static bool TryParse(string locationArgument, out API.GuessLocationRequest request)
        {
            request = null;

            // {<lon>;<lat>|<ip>|<address>}
            if (string.IsNullOrEmpty(locationArgument))
            {
                return false;
            }

            // <lon>;<lat>
            if (API.GeoPoint.TryParse(locationArgument, out API.GeoPoint location))
            {
                request = new API.GuessLocationRequest(location);
                return true;
            }

            // <ip>
            if (IPAddress.TryParse(locationArgument, out IPAddress ip))
            {
                request = new API.GuessLocationRequest(ip);
                return true;
            }

            // <address>
            if (locationArgument.Length >= AddressLengthMin)
            {
                request = new API.GuessLocationRequest(locationArgument);
                return true;
            }

            return false;
        }
    }
}