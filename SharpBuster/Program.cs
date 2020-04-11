using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
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

        //TODO: Provide descriptions
        private static (string, string, FileInfo, int) HandleCommandLine(string[] args)
        {
            var rootCmd = new RootCommand {
                new Option<string>(new [] {"--address", "--url", "-a"}, "N/A"),
                new Option<string>(new []{"--list-url", "-l"}, () => null, "N/A") {Required = false},
                new Option<FileInfo>(new []{"--log-file","-o"}, () => null, "N/A"){Required = false},
                new Option<int>(new []{"--threads","-t"}, () => 16, "N/A"){Required = false}
            };

            rootCmd.Description = "N/A";

            (string, string, FileInfo, int) result = default;
            rootCmd.Handler = CommandHandler.Create<string, string, FileInfo, int>((a, l, o, t) => { result = (a, l, o, t); });
            rootCmd.Invoke(args, new SystemConsole());

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
