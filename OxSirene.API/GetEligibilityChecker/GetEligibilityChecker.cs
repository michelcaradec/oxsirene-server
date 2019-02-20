using System;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace OxSirene.API
{
    public static class GetEligibilityChecker
    {
        public static async Task<GetEligibilityCheckerResponse> RunAsync(GetEligibilityCheckerRequest request)
        {
            return await Task.Run(() =>
                {
                    if (request == null)
                    {
                        throw new ArgumentNullException(nameof(request));
                    }
                    if (!request.IsValid)
                    {
                        throw new ArgumentException(nameof(request));
                    }

                    var scraper = ScrapProductFactory.CreateInstance(request.Page);
                    if (scraper == null)
                    {
                        throw new NotSupportedException(request.Page.Host);
                    }

                    return new GetEligibilityCheckerResponse(scraper.MarketPlaceID, scraper.JavaScriptEligibilityChecker);
                }
            );
        }
    }
}