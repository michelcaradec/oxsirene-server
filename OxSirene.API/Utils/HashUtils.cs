using System;
using System.Text;
using System.Linq;
using System.Security.Cryptography;

namespace OxSirene.API
{
    internal static class HashUtils
    {
        public static string ToMD5(string text)
        {
            using (var md5 = MD5.Create())
            {
                string hash
                    = md5.ComputeHash(Encoding.UTF8.GetBytes(text))
                    .Select(b => b.ToString("x2"))
                    .Aggregate("", (s, a) => s + a);
                return hash;
            }
        }
    }
}
