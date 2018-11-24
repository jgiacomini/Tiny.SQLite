using System;

namespace Tiny.SQLite
{
    internal sealed class InternalLogger : IDisposable
    {
        public Action<string> OnLog { get; set; }

        public void Dispose()
        {
            OnLog = null;
        }

        public void Log(string query, TimeSpan timeSpan)
        {
            string format = "mm':'ss':'fff";
            OnLog?.Invoke($"{query}{Environment.NewLine}Completed in {timeSpan.ToString(format)};");
        }
    }
}
