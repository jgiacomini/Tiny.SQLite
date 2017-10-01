using System;
using System.Collections.Generic;
using System.Text;

namespace TinySQLite
{
    internal class InternalLogger : IDisposable
    {
        public Action<string> OnLog;

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
