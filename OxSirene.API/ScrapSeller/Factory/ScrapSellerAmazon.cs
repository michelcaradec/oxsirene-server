using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace OxSirene.API.Factory
{
    internal class ScrapSellerAmazon : IScrapSeller
    {
        private const string CommercialIDPropertyName = "Numéro de registre de commerce";
        private const string CommercialNamePropertyName = "Nom commercial";

        #region IScrapSeller

        public Uri GetPageUri(string sellerID)
        {
            return new Uri(string.Format(Configuration.Instance["api:scrap_seller:amazon:seller_page"], sellerID));
        }

        private static Regex _regex_list
            = new Regex(Configuration.Instance["api:scrap_seller:amazon:pattern_list"], RegexOptions.Compiled | RegexOptions.Singleline);
        private static Regex _regex_list_items
            = new Regex(Configuration.Instance["api:scrap_seller:amazon:pattern_list_items"], RegexOptions.Compiled);
        private static Regex _regex_clean_html
            = new Regex(Configuration.Instance["api:scrap_seller:amazon:pattern_clean_html"], RegexOptions.Compiled);

        public IDictionary<string, string> GetProperties(string content)
        {
            var properties = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(content))
            {
                var match = _regex_list.Match(content);
                if (match.Success)
                {
                    string separator = Configuration.Instance["api:scrap_seller:amazon:info_separator"];
                    var items = _regex_list_items.Matches(match.Groups["list"].Value);
                    foreach (Match item in items)
                    {
                        string text = _regex_clean_html.Replace(item.Value, string.Empty);
                        var parts = text.Split(separator, StringSplitOptions.None);
                        string key = parts[0].Trim();
                        string value = parts.Length > 1 ? parts[1].Trim() : null;
                        properties[key] = value;

                        if (key.Equals(CommercialIDPropertyName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            properties.Add(ScrapSellerResponse.CommercialIDPropertyName, value);
                        }
                        else if (key.Equals(CommercialNamePropertyName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            properties.Add(ScrapSellerResponse.CommercialNamePropertyName, value);
                        }
                    }
                }
            }

            return properties;
        }

        #endregion
    }
}
