using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SharpBuster
{
    public class Buster
    {
        private const string defaultDirListUrl = "https://raw.githubusercontent.com/daviddias/node-dirbuster/master/lists/directory-list-2.3-small.txt";
        private static readonly FileInfo defaultLogFile = new FileInfo("log.txt");

        private readonly string _url;
        private readonly string _dirListUrl;
        private readonly Logger _log;
        private readonly int _maxThreads;

        private HttpClient _http;
        private StreamLineReader _listReader;

        public event EventHandler Finished;

        public Buster(string url, string directoryListUrl = null, FileInfo logFile = null, int maxThreads = 16)
        {
            if (directoryListUrl == null) directoryListUrl = defaultDirListUrl;
            if (logFile == null) logFile = defaultLogFile;

            _url = url;
            _dirListUrl = directoryListUrl;
            _log = new Logger(logFile);
            _maxThreads = maxThreads;
        }
        
        public async void Start()
        {
            _http = new HttpClient();
            _listReader = new StreamLineReader(await _http.GetStreamAsync(_dirListUrl));

            await StartChunk();
        }

        private async Task RequestDir(string dir)
        {
            int status;
            try {
                var res = await _http.GetAsync($"{_url.TrimEnd('/')}/{dir}");
                status = (int) res.StatusCode;
            }
            catch {
                status = -1;
            }

            _log.Log($"{dir}={status}");
        }

        private async Task StartChunk()
        {
            var block = new ActionBlock<string>(
                async dir => await RequestDir(dir),
                new ExecutionDataflowBlockOptions {MaxDegreeOfParallelism = _maxThreads});
            await foreach (var dir in _listReader.ReadLinesAsync(_maxThreads,
                dir => !dir.StartsWith('#') ||
                       string.IsNullOrEmpty(dir) ||
                       string.IsNullOrWhiteSpace(dir))) {
                block.Post(dir);
            }

            block.Complete();
            await block.Completion;
            await OnChunkFinished();
        }

        private async Task OnChunkFinished()
        {
            if (!_listReader.EndOfStream) {
                await StartChunk();
            }
            else {
                OnFinished();
            }
        }

        private void OnFinished()
        {
            _http.Dispose();
            _listReader.Close();
            _log.Dispose();
            Finished?.Invoke(this, EventArgs.Empty);
        }
    }
}
