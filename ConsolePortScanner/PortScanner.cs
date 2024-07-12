using System.Net.Sockets;
using System.Net;

namespace ConsolePortScanner
{
    /// <summary>
    /// Класс для сканирования портов на удаленном хосте.
    /// Class for scanning ports on a remote host.
    /// </summary>
    internal class PortScanner
    {
        private readonly string _host;
        private int _portsCheckedCount;

        /// <summary>
        /// Создает новый экземпляр класса PortScanner с указанным хостом.
        /// Creates a new instance of the PortScanner class with the specified host.
        /// </summary>
        /// <param name="hostEntry">Хост для сканирования портов.</param>
        /// <param name="hostEntry">Host to scan ports on.</param>
        public PortScanner(string hostEntry)
        {
            _host = hostEntry;
            _portsCheckedCount = 0;
        }

        /// <summary>
        /// Возвращает количество проверенных портов.
        /// Returns the number of checked ports.
        /// </summary>
        public int CheckedPorts { get { return _portsCheckedCount; } }

        /// <summary>
        /// Асинхронно сканирует порты в указанном диапазоне.
        /// Asynchronously scans ports in the specified range.
        /// </summary>
        /// <param name="startPort">Начальный порт для сканирования.</param>
        /// <param name="startPort">Starting port for scanning.</param>
        /// <param name="endPort">Конечный порт для сканирования.</param>
        /// <param name="endPort">Ending port for scanning.</param>
        /// <param name="portFoundCallback">Метод обратного вызова, вызываемый при обнаружении открытого порта.</param>
        /// <param name="portFoundCallback">Callback method called when an open port is found.</param>
        public async Task ScanPortsAsync(int startPort, int endPort, Action<int> portFoundCallback)
        {
            var tasks = new List<Task>();
            IPAddress ipAddress = await GetIPAddressAsync();

            for (int port = startPort; port <= endPort; port++)
                tasks.Add(ScanPortAsync(ipAddress, port, portFoundCallback));

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Асинхронно получает IP-адрес хоста.
        /// Asynchronously gets the host's IP address.
        /// </summary>
        /// <returns>IP-адрес хоста.</returns>
        /// <returns>Host's IP address.</returns>
        public async Task<IPAddress> GetIPAddressAsync()
        {
            string url = string.IsNullOrEmpty(_host) ? "localhost" : _host;
            IPHostEntry hostEntry = await Dns.GetHostEntryAsync(url);
            return hostEntry.AddressList[0];
        }


        private async Task ScanPortAsync(IPAddress ipAddress, int port, Action<int> portFoundCallback)
        {
            using (var client = new TcpClient())
            {
                _portsCheckedCount++;
                await Task.Yield();
                try
                {
                    await client.ConnectAsync(ipAddress, port);
                    portFoundCallback(port);
                }
                catch (SocketException)
                {
                    // Port is closed
                }                
            }
        }
    }
}