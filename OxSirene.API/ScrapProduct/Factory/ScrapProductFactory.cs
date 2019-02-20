using System;
using OxSirene.API.Factory;

namespace OxSirene.API
{
    internal static class ScrapProductFactory
    {
        public static IScrapProduct CreateInstance(Uri page)
        {
            if (page.Host.StartsWith("www.amazon.", StringComparison.InvariantCultureIgnoreCase))
            {
                return new ScrapProductAmazon();
            }
            else if (page.Host.Equals("www.cdiscount.com", StringComparison.InvariantCultureIgnoreCase))
            {
                return new ScrapProductCdiscount();
            }
            
            return null;
        }
    }
}
