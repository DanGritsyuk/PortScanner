using System.Net.Sockets;
using System.Net;

namespace ConsolePortScanner
{
    internal class PortScanner
    {
        private readonly string _host;
        private int _portsCheckedCount;

        public PortScanner(string hostEntry)
        {
            _host = hostEntry;
            _portsCheckedCount = 0;
        }

        public int CheckedPorts { get { return _portsCheckedCount; } }

        public async Task ScanPortsAsync(int startPort, int endPort, Action<int> portFoundCallback)
        {
            var tasks = new List<Task>();
            IPAddress ipAddress = await GetIPAddressAsync();

            for (int port = startPort; port <= endPort; port++)
                tasks.Add(ScanPortAsync(ipAddress, port, portFoundCallback));

            await Task.WhenAll(tasks);
        }

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