using System.Net.Sockets;
using System.Net;

namespace ConsolePortScanner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var parameters = new Dictionary<CommandLineArgument, string?>();

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--"))
                {
                    if (Enum.TryParse(args[i].Substring(2), out CommandLineArgument key))
                    {
                        string value = (i + 1 < args.Length && !args[i + 1].StartsWith("--")) ? args[i + 1] : "";
                        parameters[key] = value;
                    }
                }
            }

            await PortScanController.Startup(parameters);
        }
    }
}

//--Link router.asus.com --StartPort 80 --EndPort 443