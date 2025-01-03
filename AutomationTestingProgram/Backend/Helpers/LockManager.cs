namespace AutomationTestingProgram.Backend.Helpers
{   
    /// <summary>
    /// Class used to manage a lock dictionary with appropriate addition and deletion
    /// of entires in a thread-safe manner.
    /// </summary>
    public class LockManager<T> where T : notnull
    {
        private readonly object _bigLock;
        private readonly Dictionary<T, LockInfo> _lockMap;

        public LockManager()
        {
            _bigLock = new object();
            _lockMap = new Dictionary<T, LockInfo>();
        }

        public async Task AquireLockAsync(T key)
        {

            LockInfo value;
            lock (_bigLock)
            {
                if (!_lockMap.TryGetValue(key, out value!))
                {
                    value = new LockInfo();
                    _lockMap.Add(key, value);
                }
                else
                {
                    value.Count++;
                }
            }

            await value.Semaphore.WaitAsync();
        }

        public void ReleaseLock(T key)
        {
            lock (_bigLock)
            {
                if (_lockMap.TryGetValue(key, out var value))
                {
                    value.Semaphore.Release();

                    if (value.Count == 1)
                    {
                        _lockMap.Remove(key);
                        value.Semaphore.Dispose();
                    }
                    else
                    {
                        value.Count--;
                    }
                }
            }
        }

        public class LockInfo
        {
            public int Count { get; set; }
            public SemaphoreSlim Semaphore { get; set; }

            public LockInfo()
            {
                Semaphore = new SemaphoreSlim(1);
                Count = 1;
            }
        }

    }
}
