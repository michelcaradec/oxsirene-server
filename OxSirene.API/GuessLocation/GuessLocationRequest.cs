using System;
using System.Net;

namespace OxSirene.API
{
    public enum LocationType
    {
        IP, Coordinates, Address
    }

    public class GuessLocationRequest
    {
        /// <summary>
        /// Location type.
        /// </summary>
        public LocationType Type { get; private set; }
        /// <summary>
        /// IP address.
        /// </summary>
        public IPAddress IP { get; private set; }
        /// <summary>
        /// Geo-location.
        /// </summary>
        public GeoPoint Coordinates { get; private set; }
        /// <summary>
        /// Address.
        /// </summary>
        public string Address { get; private set; }
        public bool IsValid => IP != null || Coordinates != null || !string.IsNullOrEmpty(Address);

        public GuessLocationRequest(IPAddress ip, bool strict = true)
        {
            IP = ip;
            Type = LocationType.IP;

            if (strict && !IsValid)
            {
                throw new ArgumentException(nameof(ip));
            }
        }

        public GuessLocationRequest(GeoPoint coordinates, bool strict = true)
        {
            Coordinates = coordinates;
            Type = LocationType.Coordinates;

            if (strict && !IsValid)
            {
                throw new ArgumentException(nameof(coordinates));
            }
        }

        public GuessLocationRequest(string address, bool strict = true)
        {
            Address = address;
            Type = LocationType.Address;

            if (strict && !IsValid)
            {
                throw new ArgumentException(nameof(address));
            }
        }
    }
}