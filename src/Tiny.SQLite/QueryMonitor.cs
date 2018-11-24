using System;
using System.Diagnostics;

namespace Tiny.SQLite
{
    internal sealed class QueryMonitor : IDisposable
    {
        #region Fields
        private readonly string _query;
        private readonly Stopwatch _stopwatch;
        private InternalLogger _internalLogger;
        #endregion

        public QueryMonitor(string query, InternalLogger internalLogger)
        {
            _stopwatch = new Stopwatch();
            _internalLogger = internalLogger;
            _query = query;
            _stopwatch.Start();
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _stopwatch.Stop();
                    _internalLogger.Log(_query, _stopwatch.Elapsed);
                    _internalLogger = null;
                }

                _disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~QueryMonitor() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);

            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}