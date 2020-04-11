using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using System.Timers;

namespace SharpBuster
{
    public class Logger : IDisposable, IAsyncDisposable
    {
        private readonly StreamWriter _writer;
        private readonly bool _writeToConsole;
        private readonly ConcurrentQueue<string> _queue;
        private readonly Timer _timer;

        public Logger(FileInfo file, bool writeToConsole = true)
        {
            File = file;
            _writer = new StreamWriter(file.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write));
            _writeToConsole = writeToConsole;
            _queue = new ConcurrentQueue<string>();
            _timer = new Timer(500);
            _timer.Elapsed += OnTimerElapsed;
            _timer.Start();
        }

        public Logger(string file, bool writeToConsole = true) : this(new FileInfo(file), writeToConsole) { }

        public FileInfo File { get; }

        public void Log(string value)
        {
            _queue.Enqueue(value);
        }

        private async void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            while (!_queue.IsEmpty) {
                var canDequeue = _queue.TryDequeue(out var dir);
                if (!canDequeue) continue;

                await _writer.WriteLineAsync(dir);
                if (_writeToConsole)
                    Console.WriteLine(dir);
            }

            _timer.Start();
        }

        public void Dispose()
        {
            _writer.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            await _writer.DisposeAsync();
        }
    }
}
