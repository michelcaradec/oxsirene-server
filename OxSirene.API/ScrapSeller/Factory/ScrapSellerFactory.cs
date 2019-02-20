using System;

namespace OxSirene.API
{
    internal static class ScrapSellerFactory
    {
        public static IScrapSeller CreateInstance(string marketPlaceID)
        {
            if (marketPlaceID.Equals("amazon", StringComparison.InvariantCultureIgnoreCase))
            {
                return new Factory.ScrapSellerAmazon();
            }
            else if (marketPlaceID.Equals("cdiscount", StringComparison.InvariantCultureIgnoreCase))
            {
                return new Factory.ScrapSellerCdiscount();
            }
            
            return null;
        }
    }
}
