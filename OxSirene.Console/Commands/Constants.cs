using System;

namespace OxSirene.Console
{
    internal static class Application
    {
        public const string Version = "0.1";
    }

    internal static class CmdLineAction
    {
        public const string Help = "help";
        public const string EstimateDelivery = "delivery";
        public const string Version = "version";

        public const string Location = "location";
        public const string From = "from";
        public const string Quiet = "quiet";
    }

    internal static class OSM
    {
        public const int ZoomLevel = 18;
    }
}