using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SharpBuster
{
    public class StreamLineReader : StreamReader
    {
        public StreamLineReader(Stream stream) : base(stream) { }

        public int LinesRead { get; private set; }

        public async IAsyncEnumerable<string> ReadLinesAsync(int amount, Predicate<string> predicate)
        {
            var i = 0;
            while (i < amount) {
                var line = await ReadLineAsync();
                if (!predicate(line)) continue;
                i++;
                yield return line;
            }
        }

        public override Task<string> ReadLineAsync()
        {
            LinesRead++;
            return base.ReadLineAsync();
        }
    }
}
