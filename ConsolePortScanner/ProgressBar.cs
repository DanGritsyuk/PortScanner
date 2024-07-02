namespace ConsolePortScanner
{
    public class ProgressBar : IProgress<int>, IDisposable
    {
        private int _min;
        private int _max;
        private int _current;
        private (int, int) _startBarCursorPosition;
        private decimal _previousPercentage;
        private bool _cursorVisible;
        private bool _runProgressBar;
        private bool _isReporting;
        private string _afterText;


        public ProgressBar(int min, int max)
        {
            _min = min;
            _max = max;
            _current = _min;
            _startBarCursorPosition = (0, 0);
            _previousPercentage = 0;
            _cursorVisible = true;
            _runProgressBar = false;
            _isReporting = false;
            _afterText = "";
        }

        public void AddTextAfterBar(string text) =>
            _afterText += string.IsNullOrEmpty(_afterText) ? ("\n\n" + text + "\n") : (text + "\n");

        public void Report(int value)
        {
            if (!_isReporting)
            {
                lock (this)
                {
                    _isReporting = true;

                    if (!_runProgressBar)
                    {
                        _startBarCursorPosition = Console.GetCursorPosition();
                        _runProgressBar = true;
                    }

                    _current = value;
                    decimal percentage = (_current - _min + 1) / (decimal)(_max - _min + 1);
                    if (percentage != _previousPercentage)
                    {
                        _previousPercentage = percentage;
                        HideCursor();
                        UpdateProgress();
                        ShowCursor();
                    }

                    _isReporting = false;
                }
            }
        }

        public void Dispose()
        {
            Console.WriteLine();
            _runProgressBar = false;
        }

        private void UpdateProgress()
        {
            int width = Console.WindowWidth - 20;

            Console.SetCursorPosition(_startBarCursorPosition.Item1, _startBarCursorPosition.Item2);
            Console.Write("[");

            int position = 1;
            int progressWidth = (int)(_previousPercentage * width);
            for (int i = 0; i < progressWidth; i++)
            {
                Console.BackgroundColor = ConsoleColor.Green;
                Console.CursorLeft = position++;
                Console.Write(" ");
            }
            Console.BackgroundColor = ConsoleColor.Black;

            for (int i = position; i <= width; i++)
            {
                Console.CursorLeft = position++;
                Console.Write(" ");
            }

            Console.CursorLeft = width + 1;



            Console.Write($"] {_previousPercentage:P0}{_afterText}");
        }

        private void HideCursor()
        {
            if (_cursorVisible)
            {
                Console.CursorVisible = false;
                _cursorVisible = false;
            }
        }

        private void ShowCursor()
        {
            if (!_cursorVisible)
            {
                Console.CursorVisible = true;
                _cursorVisible = true;
            }
        }
    }
}
