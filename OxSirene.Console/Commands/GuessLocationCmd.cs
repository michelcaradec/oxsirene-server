using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace OxSirene.Console
{
    internal static class GuessLocationCmd
    {
        public static async Task ProcessAsync(string locationArgument, bool verbose)
        {
            if (string.IsNullOrEmpty(API.Configuration.Instance.RequestHeaderFrom))
            {
                UI.PrintFromWarning();
            }

            if (GuessLocationUtils.TryParse(locationArgument, out API.GuessLocationRequest requestLocation))
            {
                var responseLocation = await API.GuessLocation.RunAsync(requestLocation);
                if (responseLocation.IsValid)
                {
                    UI.PrintInfo($"Address: {responseLocation.Address}");
                    UI.PrintInfo($"Coordinates: {responseLocation.Coordinates}");
                    UI.PrintInfo($"https://www.openstreetmap.org/#map={OSM.ZoomLevel}/{responseLocation.Coordinates.Lat}/{responseLocation.Coordinates.Lon}");
                }
                else
                {
                    UI.PrintError("Failed to guess location");
                }
            }
            else
            {
                UI.PrintError($"Invalid argument --{CmdLineAction.Location}");
            }
        }
    }
}