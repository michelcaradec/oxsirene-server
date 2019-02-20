using System;

namespace OxSirene.Console
{
    internal static class UI
    {
        private const string Executable = "OxSirene";

        public static void PrintBanner()
        {
            System.Console.WriteLine($"OxSirene v{Application.Version}");
            System.Console.WriteLine("Author: Michel Caradec");
            System.Console.WriteLine("Repository: https://github.com/michelcaradec/oxsirene-server");
            System.Console.WriteLine();
        }

        public static void PrintHint()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("  " + Executable + " --help");
            System.Console.WriteLine("    Show help.");
        }

        public static void PrintHelp()
        {
            System.Console.WriteLine("Usage:");
            System.Console.WriteLine("  " + Executable + " --help");
            System.Console.WriteLine("    Show this help.");
            System.Console.WriteLine("  " + Executable + " --version");
            System.Console.WriteLine("    Show version.");
            System.Console.WriteLine("  " + Executable + " --delivery PRODUCT_URI [--from INFO]");
            System.Console.WriteLine("    Estimate delivery for a product.");
            System.Console.WriteLine();
            System.Console.WriteLine("Options:");
            System.Console.WriteLine("  --quiet");
            System.Console.WriteLine("    Prevent banner display.");
            // System.Console.WriteLine();
            // System.Console.WriteLine("Samples:");
            // System.Console.WriteLine("  " + Executable + @" --races ""https://www.klikego.com/resultats/tkb-trail-du-kreiz-breizh-2017/1477908279468-1""");
            // System.Console.WriteLine("  " + Executable + @" --scrap ""https://www.klikego.com/resultats/tkb-trail-du-kreiz-breizh-2017/1477908279468-1"" --race ""TKB 28 - AXA Auréart Le Galliar"" --output tkb_2017.tsv");
        }

        public static void PrintVersion()
        {
            System.Console.WriteLine($"v{Application.Version}");
        }

        public static void PrintFromWarning()
        {
            var color = System.Console.ForegroundColor;

            System.Console.ForegroundColor = ConsoleColor.DarkYellow;
            System.Console.WriteLine("No value was given to --from argument.");
            System.Console.WriteLine("A best practice is to provide one when submitting a request.");
            System.Console.WriteLine("Examples:");
            System.Console.WriteLine(@"  --from ""jdoe@mail.com""");
            System.Console.WriteLine(@"  --from ""John Doe, jdoe@mail.com""");
            System.Console.WriteLine();
            
            System.Console.ForegroundColor = color;
        }

        public static void PrintInfo(string format, params object[] args)
        {
            Print(ConsoleColor.Green, format, args);
        }

        public static void PrintWarning(string format, params object[] args)
        {
            Print(ConsoleColor.DarkYellow, format, args);
        }

        public static void PrintError(string format, params object[] args)
        {
            Print(ConsoleColor.Red, "ERROR - " + format, args);
        }

        private static void Print(ConsoleColor color, string format, params object[] args)
        {
            var save = System.Console.ForegroundColor;

            System.Console.ForegroundColor = color;
            System.Console.WriteLine(format, args);

            System.Console.ForegroundColor = save;
        }
    }
}
