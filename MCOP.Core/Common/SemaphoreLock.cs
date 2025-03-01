using System.Collections.Concurrent;

namespace MCOP.Core.Common
{
    public class SemaphoreLock
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public static async Task<IDisposable> LockAsync(string key)
        {
            var semaphore = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();

            return new ReleaseWrapper(() => semaphore.Release());
        }

        private class ReleaseWrapper : IDisposable
        {
            private readonly Action _release;

            public ReleaseWrapper(Action release)
            {
                _release = release;
            }

            public void Dispose()
            {
                _release();
            }
        }
    }
}
