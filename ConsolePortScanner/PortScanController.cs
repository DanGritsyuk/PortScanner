using System.Net.Sockets;
using System.Net;

namespace ConsolePortScanner
{
    internal static class PortScanController
    {
        internal static async Task Startup(Dictionary<CommandLineArgument, string?> parameters)
        {
            int startPort, endPort = 0;
            PortScanner scanner;

            if (!parameters.ContainsKey(CommandLineArgument.Link))
            {
                Console.Write("Enter domain: ");
                parameters[CommandLineArgument.Link] = Console.ReadLine();
            }
            if (!parameters.ContainsKey(CommandLineArgument.StartPort))
            {
                Console.Write("Enter start port: ");
                parameters[CommandLineArgument.StartPort] = Console.ReadLine();
            }
            if (!parameters.ContainsKey(CommandLineArgument.EndPort))
            {
                Console.Write("Enter end port: ");
                parameters[CommandLineArgument.EndPort] = Console.ReadLine();
            }

            if (!int.TryParse(parameters[CommandLineArgument.StartPort], out startPort)
                || !int.TryParse(parameters[CommandLineArgument.EndPort], out endPort)
                || startPort > endPort)
            {
                Console.WriteLine("Invalid port numbers. Scanning full diapason.");
                startPort = endPort = 0;
            }

            string url = string.IsNullOrEmpty(parameters[CommandLineArgument.Link]) ? "localhost" : parameters[CommandLineArgument.Link]!;
            IPHostEntry hostEntry = await Dns.GetHostEntryAsync(url);

            if (startPort > 0 && endPort > 0)
                scanner = new PortScanner(hostEntry, startPort, endPort);
            else
                scanner = new PortScanner(hostEntry);

            Console.WriteLine($"Scanning ports for {url} ({hostEntry.AddressList[0]})...\n");

            try
            {
                bool findOpenPort = false;

                using (var progressBar = new ProgressBar(0, scanner.TotalPorts))
                {
                    bool firstLine = true;
                    await foreach (var port in scanner.FindOpenPortsAsync(progressBar))
                    {
                        if (firstLine)
                        {
                            Console.WriteLine("\n");
                            firstLine = false;
                        }

                        Console.WriteLine($"Found open port: {port}");
                        findOpenPort = true;
                    }
                }

                if (findOpenPort)
                    Console.WriteLine("Scanning done!");
                else
                    Console.WriteLine("No open ports found.");
            }
            catch (SocketException)
            {
                Console.WriteLine("The hostname could not be resolved.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex}");
            }
            finally
            {
                Console.WriteLine("The program has completed. Press any key...");
                Console.ReadKey();
            }
        }
    }
}
