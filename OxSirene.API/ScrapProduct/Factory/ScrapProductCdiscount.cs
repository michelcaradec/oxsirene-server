using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;
using System.Net.Http;

namespace OxSirene.API.Factory
{
    internal class ScrapProductCdiscount : IScrapProduct
    {
        #region IScrapProduct

        public string MarketPlaceID => "cdiscount";

        /// <summary>
        /// ScrapProductCdiscount.js encoded in Base64.
        /// </summary>
        private string _javaScript = "ZnVuY3Rpb24gY2hlY2soY29udGVudCkgew0KICAgIGxldCBwYXR0ZXJuX29mZmVyX2xpc3QgPSAiKDx1bCBjbGFzcz1cXFwib2ZmZXJzTGlzdCBqc09mZmVyc0xpc3RcXFwiW14+XSs+KSI7DQogICAgbGV0IHBhdHRlcm5fb2ZmZXJfbGlzdF91cmkgPSAiKGRhdGEtdXJsPVxcXCIpKFteXFxcIl0rKSI7DQoNCiAgICBsZXQgcmVnZXhfb2ZmZXJfbGlzdCA9IG5ldyBSZWdFeHAocGF0dGVybl9vZmZlcl9saXN0KTsNCiAgICBsZXQgcmVnZXhfb2ZmZXJfbGlzdF91cmkgPSBuZXcgUmVnRXhwKHBhdHRlcm5fb2ZmZXJfbGlzdF91cmkpOw0KDQogICAgc2VsbGVycyA9IFtdOw0KDQogICAgbGV0IG1hdGNoZXMgPSBjb250ZW50Lm1hdGNoKHJlZ2V4X29mZmVyX2xpc3QpOw0KICAgIGlmIChtYXRjaGVzKSB7DQogICAgICAgIGZvciAobGV0IG1hdGNoIG9mIG1hdGNoZXMpIHsNCiAgICAgICAgICAgIGxldCBtYXRjaDIgPSBtYXRjaC5tYXRjaChyZWdleF9vZmZlcl9saXN0X3VyaSk7DQogICAgICAgICAgICBpZiAobWF0Y2gyKSB7DQogICAgICAgICAgICAgICAgc2VsbGVycy5wdXNoKG1hdGNoMlsxXSk7DQogICAgICAgICAgICB9DQogICAgICAgIH0NCiAgICB9DQoNCiAgICByZXR1cm4gc2VsbGVycy5sZW5ndGggPiAwOw0KfQ==";

        public string JavaScriptEligibilityChecker => Encoding.UTF8.GetString(Convert.FromBase64String(_javaScript));

        private static Regex _regex_title
            = new Regex(Configuration.Instance["api:scrap_product:cdiscount:pattern_title"], RegexOptions.Compiled);

        public string GetProductName(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }

            var match = _regex_title.Match(content);
            if (match.Success)
            {
                return HttpUtility.HtmlDecode(match.Groups["product"].Value.Trim());
            }

            return null;
        }

        private static Regex _regex_offer_list
            = new Regex(Configuration.Instance["api:scrap_product:cdiscount:pattern_offer_list"], RegexOptions.Compiled);

        private static Regex _regex_offer_list_uri
            = new Regex(Configuration.Instance["api:scrap_product:cdiscount:pattern_offer_list_uri"], RegexOptions.Compiled);

        private static Regex _regex_seller
            = new Regex(Configuration.Instance["api:scrap_product:cdiscount:pattern_seller"], RegexOptions.Compiled);          

        public IEnumerable<string> GetSellers(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                yield break;
            }

            var match = _regex_offer_list.Match(content);
            if (match.Success)
            {
                var matchUri = _regex_offer_list_uri.Match(match.Groups["offer_list"].Value);
                if (matchUri.Success)
                {
                    var uri = new Uri(new Uri("https://www.cdiscount.com"), matchUri.Groups["offer_list_uri"].Value);
                    string offerListContent = WebUtils.GetWebPage(uri);
                    
                    foreach (Match matchSeller in _regex_seller.Matches(offerListContent))
                    {
                        yield return matchSeller.Groups["seller"].Value;
                    }
                }
            }
        }

        #endregion
    }
}
