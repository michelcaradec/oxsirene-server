using System;
using System.Diagnostics;

namespace OxSirene.API
{
    public class GuessLocationResponse
    {
        /// <summary>
        /// Geo-location.
        /// </summary>
        public GeoPoint Coordinates { get; private set; }
        /// <summary>
        /// Address.
        /// </summary>
        public string Address { get; private set; }
        public bool IsValid => Coordinates != null && !string.IsNullOrEmpty(Address);

        public static GuessLocationResponse Null => new GuessLocationResponse();

        [DebuggerStepThrough]
        private GuessLocationResponse()
        {
        }

        [DebuggerStepThrough]
        public GuessLocationResponse(GeoPoint coordinates, string address, bool strict = true)
        {
            Coordinates = coordinates;
            Address = address;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}