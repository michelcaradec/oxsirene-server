using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web;
using System.Net.Http;

namespace OxSirene.API.Factory
{
    internal class ScrapProductAmazon : IScrapProduct
    {
        #region IScrapProduct

        public string MarketPlaceID => "amazon";

        /// <summary>
        /// ScrapProductAmazon.js encoded in Base64.
        /// </summary>
        private string _javaScript = "ZnVuY3Rpb24gY2hlY2soY29udGVudCkgew0KICAgIGxldCBwYXR0ZXJuX2hyZWZfMSA9ICJbXFxcIiddKFxcL2dwXFwvaGVscFxcL3NlbGxlclxcL2F0LWEtZ2xhbmNlXFwuaHRtbClbXlxcXCInXSpbXFxcIiddIjsNCiAgICBsZXQgcGF0dGVybl9ocmVmXzIgPSAiW1xcXCInXShcXC9ncFxcL2FhZ1xcL21haW5cXC9yZWY9KVteXFxcIiddKltcXFwiJ10iOw0KICAgIGxldCBwYXR0ZXJuX3NlbGxlciA9ICIoc2VsbGVyPSkoW15cXFwiJyZdKikiOw0KDQogICAgbGV0IHJlZ2V4X2hyZWZfMSA9IG5ldyBSZWdFeHAocGF0dGVybl9ocmVmXzEsICJnIik7DQogICAgbGV0IHJlZ2V4X2hyZWZfMiA9IG5ldyBSZWdFeHAocGF0dGVybl9ocmVmXzIpOw0KICAgIGxldCByZWdleF9zZWxsZXIgPSBuZXcgUmVnRXhwKHBhdHRlcm5fc2VsbGVyKTsNCg0KICAgIHNlbGxlcnMgPSBbXTsNCg0KICAgIGZvciAobGV0IHJlZ2V4X2hyZWYgb2YgW3JlZ2V4X2hyZWZfMSwgcmVnZXhfaHJlZl8yXSkgew0KICAgICAgICBsZXQgbWF0Y2hlcyA9IGNvbnRlbnQubWF0Y2gocmVnZXhfaHJlZik7DQogICAgICAgIGlmICghbWF0Y2hlcykgew0KICAgICAgICAgICAgY29udGludWU7DQogICAgICAgIH0NCg0KICAgICAgICBmb3IgKGxldCBtYXRjaCBvZiBtYXRjaGVzKSB7DQogICAgICAgICAgICBsZXQgbWF0Y2gyID0gbWF0Y2gubWF0Y2gocmVnZXhfc2VsbGVyKTsNCiAgICAgICAgICAgIGlmIChtYXRjaDIpIHsNCiAgICAgICAgICAgICAgICBzZWxsZXJzLnB1c2gobWF0Y2gyWzJdKTsNCiAgICAgICAgICAgIH0NCiAgICAgICAgfQ0KICAgIH0NCg0KICAgIHJldHVybiBzZWxsZXJzLmxlbmd0aCA+IDA7DQp9";

        public string JavaScriptEligibilityChecker => Encoding.UTF8.GetString(Convert.FromBase64String(_javaScript));

        private static Regex _regex_title
            = new Regex(Configuration.Instance["api:scrap_product:amazon:pattern_title"], RegexOptions.Compiled);

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

        private static Regex _regex_href_1
            = new Regex(Configuration.Instance["api:scrap_product:amazon:pattern_href_1"], RegexOptions.Compiled);
        private static Regex _regex_href_2
            = new Regex(Configuration.Instance["api:scrap_product:amazon:pattern_href_2"], RegexOptions.Compiled);
        private static Regex _regex_seller
            = new Regex(Configuration.Instance["api:scrap_product:amazon:pattern_seller"], RegexOptions.Compiled);

        public IEnumerable<string> GetSellers(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                yield break;
            }

            foreach (var regex_href in new[] { _regex_href_1, _regex_href_2 })
            {
                foreach (Match match in regex_href.Matches(content))
                {
                    var match2 = _regex_seller.Match(match.Value);
                    if (match2.Success && match.Groups.Count == 2)
                    {
                        yield return match2.Groups.Last().Value;
                    }
                }
            }
        }

        #endregion
    }
}
