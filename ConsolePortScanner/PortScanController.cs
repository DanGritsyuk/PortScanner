using System.Net.Sockets;
using System.Diagnostics;

namespace ConsolePortScanner
{
    /// <summary>
    /// Класс для управления сканированием портов.
    /// Class for controlling port scanning.
    /// </summary>
    internal static class PortScanController
    {
        /// <summary>
        /// Запускает сканирование портов на основе переданных параметров командной строки.
        /// Launches port scanning based on the passed command line parameters.
        /// </summary>
        /// <param name="parameters">Словарь параметров командной строки.</param>
        /// <param name="parameters">Dictionary of command line parameters.</param>
        internal static async Task Startup(Dictionary<CommandLineArgument, string?> parameters)
        {
            int startPort, endPort, totalPorts = 0;

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
                || startPort > endPort || startPort < 1 || endPort > 65535)
            {
                Console.WriteLine("Invalid port numbers. Scanning full diapason.");
                startPort = 1;
                endPort = totalPorts = 65535;
            }
            else
            {
                totalPorts = endPort - startPort + 1;
            }

            var scanner = new PortScanner(parameters[CommandLineArgument.Link] ?? "");

            Console.WriteLine($"Scanning ports for {parameters[CommandLineArgument.Link]} ({await scanner.GetIPAddressAsync()})...\n");

            try
            {
                bool findOpenPort = false;

                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                using (var progressBar = new ProgressBar(0, totalPorts))
                {
                    Action<int> openPortCallback = (p) =>
                    {
                        findOpenPort = true;
                        progressBar.AddTextAfterBar($"Found open port: {p}");
                    };

                    var task = scanner.ScanPortsAsync(startPort, endPort, openPortCallback);
                    while (true)
                    {
                        progressBar.Report(scanner.CheckedPorts);
                        await Task.Delay(100);
                        if (scanner.CheckedPorts >= totalPorts) break;
                    }

                    await task;
                    progressBar.Report(totalPorts);
                }

                stopwatch.Stop();
                Console.WriteLine(stopwatch.ElapsedMilliseconds);

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
