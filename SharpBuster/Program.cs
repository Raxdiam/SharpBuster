using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.RegularExpressions;

namespace SharpBuster
{
    internal class Program
    {
        private static bool _exit;

        private static void Main(string[] args)
        {
            var (url, listUrl, logFile, threads) = HandleCommandLine(args);
            if (!IsValidUrl(url, out _)) return;

            var buster = new Buster(url, listUrl, logFile, threads);
            buster.Finished += (s, e) => _exit = true;
            buster.Start();

            while (!_exit) { }
        }

        //TODO: Handle local dir list file
        private static (string, string, FileInfo, int) HandleCommandLine(string[] args)
        {
            var rootCmd = new RootCommand {
                new Option<string>(new[] {"--address", "--url", "-a"}, "Target URL") {Required = true},
                new Option<string>(new[] {"--list-url", "-l"}, () => null, "URL to list of dirs"),
                new Option<FileInfo>(new[] {"--log-file", "-o"}, () => null, "Path to output log file"),
                new Option<int>(new[] {"--threads", "-t"}, () => 16, "Maximum number of threads")
            };

            rootCmd.Description = "C# .NET Core implementation of DirBuster";

            (string, string, FileInfo, int) result = default;
            rootCmd.Handler = CommandHandler.Create<string, string, FileInfo, int>((a, l, o, t) => { result = (a, l, o, t); });
            rootCmd.Invoke(args);

            return result;
        }

        private static bool IsValidUrl(string url, out Uri uri)
        {
            if (url == null) {
                uri = null;
                return false;
            }

            if (!Regex.Match(url, @"^https?:\/\/", RegexOptions.IgnoreCase).Success)
                url = $"http://{url}";

            if (Uri.TryCreate(url, UriKind.Absolute, out uri))
                return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;

            return false;
        }
    }
}
