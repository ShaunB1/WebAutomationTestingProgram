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
        private readonly SemaphoreSlim _limit;

        public LockManager(int Limit)
        {
            _bigLock = new object();
            _lockMap = new Dictionary<T, LockInfo>();
            _limit = new SemaphoreSlim(Limit);
        }

        /*
         * Manages a dictionary of locks, allowing the lock for each entry to be accessed one at a time.
         * A max limit of total locks may be in use at a time.
         * 
         * The idea:
         * Say 5 different request types: A, B, C, D, E with a limit of 3
         * 
         * This means 3 different processes can happen at a time.
         * 
         * Ex 1:
         * Received: A B C
         * Round 1:  Complete
         * 
         * Ex 2:
         * Received: A B C D E
         * Round 1:  D E -> (A B C Complete)
         * Round 2:  Complete
         * 
         * Ex 3:
         * Received: A A
         * Round 1:  A -> (First A Completes)
         * Round 2:  Complete
         * 
         * Ex 4:
         * Received: A A A A
         * Round 1:  A A A   -> (First A Completes)
         * Round 2:  A A     -> (Second A Completes)
         * Round 3:  A       -> (Third A Completes)
         * Round 4:  Complete
         * 
         * Ex 5:
         * Received: A A A A B B C D D D E E E E E
         * Round 1:  A A A   B     D D D E E E E E -> (A B C process. C completes. First A, B completes)
         * Round 2:  A A           D D   E E E E E -> (A B D process. B completes. Second A, First D completes)
         * Round 3:  A             D     E E E E   -> (A D E process. A, D completes. First E completes)
         * Round 4:                      E E E     -> (Second E completes)
         * Round 5:                      E E       -> (Third E completes)
         * Round 6:                      E         -> (Fourth E completes)
         * Round 7: Complete
         * 
         * 
         */

        public async Task AquireLockAsync(T key)
        {

            bool newEntry = false;
            LockInfo value;
            lock (_bigLock)
            {
                if (!_lockMap.TryGetValue(key, out value!))
                {
                    value = new LockInfo();
                    _lockMap.Add(key, value);

                    newEntry = true;
                    value.Semaphore.Wait(); // Will be able to immediatelly block the semaphore
                }
                else
                {
                    value.Count++;
                }
            }

            if (newEntry)
            {
                await _limit.WaitAsync();
                value.Semaphore.Release();
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
                        _limit.Release();
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
