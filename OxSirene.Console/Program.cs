using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using API = OxSirene.API;
using System.Net.Http;

namespace OxSirene.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var cmd = new CmdLineArguments(args);

            bool verbose = !cmd.ContainsKey(CmdLineAction.Quiet);
            if (verbose)
            {
                UI.PrintBanner();
            }

            string from = cmd.GetValueOrDefault(CmdLineAction.From);
            if (!string.IsNullOrEmpty(from))
            {
                API.Configuration.Instance.RequestHeaderFrom = from;
            }

            if (cmd.ContainsKey(CmdLineAction.EstimateDelivery))
            {
                // --delivery <uri> [--location {<lon>;<lat>|<ip>|<address>}] [--from <from>]
                EstimateDeliveryCmd.ProcessAsync(
                    cmd.GetValueOrDefault(CmdLineAction.EstimateDelivery),
                    cmd.GetValueOrDefault(CmdLineAction.Location),
                    verbose
                ).GetAwaiter().GetResult();
            }
            else if (cmd.ContainsKey(CmdLineAction.Location))
            {
                // [--location {<lon>;<lat>|<ip>|<address>}] [--from <from>]
                GuessLocationCmd.ProcessAsync(
                    cmd.GetValueOrDefault(CmdLineAction.Location),
                    verbose
                ).GetAwaiter().GetResult();
            }
            else if (cmd.ContainsKey(CmdLineAction.Help))
            {
                // --help
                UI.PrintHelp();
            }
            else if (cmd.ContainsKey(CmdLineAction.Version))
            {
                // --version
                UI.PrintVersion();
            }
            else
            {
                UI.PrintHint();
            }
        }
    }
}
