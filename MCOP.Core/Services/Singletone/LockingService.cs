using System.Collections.Concurrent;

namespace MCOP.Core.Services.Singletone
{
    public interface ILockingService
    {
        Task<IDisposable> AcquireLockAsync<TKey>(TKey key) where TKey : notnull;
    }

    public class LockingService : ILockingService
    {
        private readonly ConcurrentDictionary<object, SemaphoreSlim> _semaphores = new();

        public async Task<IDisposable> AcquireLockAsync<TKey>(TKey key) where TKey : notnull
        {
            var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            return new ReleaseLockOnDispose<TKey>(key, semaphore, this);
        }

        private void ReleaseLock<TKey>(TKey key, SemaphoreSlim semaphore) where TKey : notnull
        {
            semaphore.Release();
            _semaphores.TryRemove(key, out _);
        }

        private class ReleaseLockOnDispose<TKey> : IDisposable where TKey : notnull
        {
            private readonly TKey _key;
            private readonly SemaphoreSlim _semaphore;
            private readonly LockingService _lockingService;

            public ReleaseLockOnDispose(TKey key, SemaphoreSlim semaphore, LockingService lockingService) 
            {
                _key = key;
                _semaphore = semaphore;
                _lockingService = lockingService;
            }

            public void Dispose()
            {
                _lockingService.ReleaseLock(_key, _semaphore);
            }
        }
    }
}
