using System;
using System.Threading;

namespace ConsolePortScanner
{
    public class ProgressBar : IProgress<int>, IDisposable
    {
        private int _min;
        private int _max;
        private int _current;
        private decimal _previousPercentage;
        private bool _cursorVisible;        
        private bool _isReporting;

        public ProgressBar(int min, int max)
        {
            _min = min;
            _max = max;
            _current = _min;
            _previousPercentage = 0;
            _cursorVisible = true;
            _isReporting = false;
        }

        public void Report(int value)
        {
            if (!_isReporting)
            {
                lock (this)
                {
                    _isReporting = true;
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
        }

        private void UpdateProgress()
        {
            int width = Console.WindowWidth - 20;

            Console.CursorLeft = 0;
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
            Console.Write($"] {_previousPercentage:P0}");
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
