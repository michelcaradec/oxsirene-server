using System;
using System.Diagnostics;

namespace OxSirene.API
{
    public class GetSireneAccessTokenRequest
    {
        public bool IsCheckCache { get; private set; }
        public bool IsValid => true;

        [DebuggerStepThrough]
        public GetSireneAccessTokenRequest(bool checkCache = true)
        {
            IsCheckCache = checkCache;

            if (!IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}
