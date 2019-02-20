using System;
using System.Collections.Generic;

namespace OxSirene.API
{
    internal interface IScrapSeller
    {
        Uri GetPageUri(string sellerID);
        
        IDictionary<string, string> GetProperties(string content);
    }
}
