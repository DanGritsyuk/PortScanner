using System.Net.Sockets;
using System.Net;

namespace ConsolePortScanner
{
    internal class PortScanner
    {
        private IPHostEntry _hostEntry;
        private int _startPort;
        private int _endPort;
        private int _portsCheckedCount;
        private Queue<Action> _progressQueue;


        public PortScanner(IPHostEntry hostEntry) : this(hostEntry, 1, 65535) { }

        public PortScanner(IPHostEntry hostEntry, int startPort, int endPort)
        {
            _hostEntry = hostEntry;
            _startPort = startPort;
            _endPort = endPort;
            _portsCheckedCount = 0;
            _progressQueue = new Queue<Action>();
        }

        public int TotalPorts { get { return _endPort - _startPort + 1; } }

        public async IAsyncEnumerable<int> FindOpenPortsAsync(IProgress<int> progress)
        {
            List<Task<int>> tasks = new List<Task<int>>();
            List<int> openPorts = new List<int>();

            for (int port = _startPort; port <= _endPort; port++)
            {
                _progressQueue.Enqueue(() => progress.Report(++_portsCheckedCount));
                tasks.Add(ScanPortAsync(_hostEntry.AddressList[0].ToString(), port, progress));
            }

            await Task.WhenAll(tasks);
            progress.Report(TotalPorts);

            _progressQueue.Clear();

            foreach (var task in tasks)
            {
                if (task.Result != -1)
                {
                    yield return task.Result;
                }
            }
        }

        private async Task<int> ScanPortAsync(string host, int port, IProgress<int> progress)
        {
            using (var client = new TcpClient())
            {
                await ExecuteProgressQueueAsync();
                try
                {
                    await client.ConnectAsync(host, port);

                    return port;
                }
                catch (SocketException)
                {
                    return -1;
                }
            }


        }

        private async Task ExecuteProgressQueueAsync()
        {
            while (true)
            {
                Action? action = null;
                lock (_progressQueue)
                {
                    if (_progressQueue.Count > 0)
                        action = _progressQueue.Dequeue();
                }

                if (action != null)
                    action.Invoke();
                else
                    break;

                await Task.Yield();
            }
        }
    }
}