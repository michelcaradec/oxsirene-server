using System;
using System.Net;
using System.Diagnostics;

namespace OxSirene.API
{
    public class GuessIPLocationRequest
    {
        /// <summary>
        /// IP address.
        /// </summary>
        public IPAddress IP { get; private set; }
        public bool IsValid => IP != null;

        [DebuggerStepThrough]
        public GuessIPLocationRequest(IPAddress ip, bool strict = true)
        {
            IP = ip;

            if (strict && !IsValid)
            {
                throw new ArgumentException();
            }
        }
    }
}
