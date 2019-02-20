using System;
using System.IO;

namespace OxSirene.API
{
    internal static class LocalCacheUtils
    {
        public static string GetFullPath(string filename) =>
            string.IsNullOrEmpty(filename)
            ? null
            : Path.Combine(Configuration.Instance.LocaCacheRoot, filename);
    }
}