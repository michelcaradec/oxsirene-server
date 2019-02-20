using System;

namespace OxSirene.API
{
    internal static class GreatCircleUtils
    {
        /// <remarks>
        /// https://en.wikipedia.org/wiki/Earth_radius
        /// </remarks>
        private const double EarthRadiusKm = 6378D;
        
        private static double ToRadian(double degree) => degree * Math.PI / 180D;

        public static double GetGreatCircle_v1(GeoPoint from, GeoPoint to)
        {
            double fromLon = ToRadian(from.Lon);
            double fromLat = ToRadian(from.Lat);
            double toLon = ToRadian(to.Lon);
            double toLat = ToRadian(to.Lat);

            return
                EarthRadiusKm
                * ( Math.PI / 2
                    - Math.Asin(
                        Math.Sin(toLat) * Math.Sin(fromLat)
                        + Math.Cos(toLon - fromLon)
                        * Math.Cos(toLat)
                        * Math.Cos(fromLat)
                    )
                );
        }

        public static double GetGreatCircle_v2(GeoPoint from, GeoPoint to)
        {
            double fromLon = ToRadian(from.Lon);
            double fromLat = ToRadian(from.Lat);
            double toLon = ToRadian(to.Lon);
            double toLat = ToRadian(to.Lat);

            var a
                = Math.Pow(Math.Sin((toLat - fromLat) / 2), 2)
                + Math.Cos(fromLat)
                * Math.Cos(toLat)
                * Math.Pow(Math.Sin((toLon - fromLon) / 2), 2);

            return EarthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }
    }
}
