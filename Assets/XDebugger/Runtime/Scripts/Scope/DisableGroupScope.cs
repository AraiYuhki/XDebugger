using System;

namespace Xeon.XDebugger.Model
{
    public sealed class DisableGroupScope : IDisposable
    {
        private readonly Action onDisposed;
        private bool disposed;

        private DisableGroupScope(Action onEnter, Action onDisposed)
        {
            onEnter?.Invoke();
            this.onDisposed = onDisposed;
        }

        public static DisableGroupScope Create(Action onEnter, Action onDisposed)
            => new DisableGroupScope(onEnter, onDisposed);

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            onDisposed?.Invoke();
        }
    }
}
