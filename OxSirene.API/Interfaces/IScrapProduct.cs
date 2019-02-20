using System;
using System.Collections.Generic;
using System.Net.Http;

namespace OxSirene.API
{
    internal interface IScrapProduct
    {
        string MarketPlaceID { get; }
        
        /// <summary>
        /// JavaScript for checking product page eligibility.
        /// </summary>
        /// <value>
        /// A string containing a JavaScript function declaration of the form:
        /// <c>function check(content) { ... }</c>
        /// The Function must return a boolean.
        /// </value>
        string JavaScriptEligibilityChecker { get; }

        string GetProductName(string content);

        IEnumerable<string> GetSellers(string content);
    }
}
