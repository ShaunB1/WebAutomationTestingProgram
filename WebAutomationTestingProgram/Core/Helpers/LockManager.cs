namespace WebAutomationTestingProgram.Core.Helpers
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

        /// <summary>
        /// Initialize a <see cref="LockManager{T}"/> class with a limit on total # of concurrent locks.
        /// </summary>
        /// <param name="limit"></param>
        public LockManager(int limit)
        {
            _bigLock = new object();
            _lockMap = new Dictionary<T, LockInfo>();
            _limit = new SemaphoreSlim(limit);
        }

        /// <summary>
        /// Initialize a <see cref="LockManager{T}"/> class without any limit on # of concurrent locks.
        /// </summary>
        public LockManager()
        {
            _bigLock = new object();
            _lockMap = new Dictionary<T, LockInfo>();
            _limit = new SemaphoreSlim(int.MaxValue); // Simulating infinite slots
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
         * In addition, cancellations are also handled (optional)
         * If a cancellation token is passed when calling AquireLockAsync(),
         * and the token is cancelled, LockManager will handle everything
         * and return an OperationCancelledException.
         * 
         * Note: If you cancel after aquring the lock, must ensure that
         * you call ReleaseLock.
         * 
         * CancellationTokens are added to ensure smooth instant cancellation
         * while waiting for a slot.
         * 
         * 
         */

        public async Task AquireLockAsync(T key, CancellationToken? token = null)
        { 
            LockInfo value;
            lock (_bigLock)
            {
                if (!_lockMap.TryGetValue(key, out value!))
                {
                    value = new LockInfo(_limit);
                    _lockMap.Add(key, value);

                    _ = value.WaitTurnAsync();
                }
                else
                {
                    value.Count++;
                }
            }

            if (token != null)
            {
                try
                {
                    await value.Semaphore.WaitAsync(token.Value);
                }
                catch (OperationCanceledException)
                {
                    lock (_bigLock)
                    {
                        if (value.Count == 1)
                        {
                            value.Dispose();
                            _lockMap.Remove(key);                            
                        }
                        else
                        {
                            value.Count--;
                        }
                    }

                    throw;
                }
            }
            else
            {
                await value.Semaphore.WaitAsync();
            }
            
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
                        value.Dispose();
                        _lockMap.Remove(key);
                    }
                    else
                    {
                        value.Count--;
                    }
                }
            }
        }

        /*
         * Verify that cancellations work with LockManager         * 
         * Work on Fairness Semaphores
         */

        public class LockInfo
        {
            public int Count { get; set; }
            public SemaphoreSlim Semaphore { get; set; }
            public CancellationTokenSource Source { get; set; }

            private SemaphoreSlim Limit;
            private bool limitAquired;
            private SemaphoreSlim Lock; // Used to ensure no concurrency issues with dispose and WaitTurnAsyc

            public LockInfo(SemaphoreSlim Limit)
            {
                Semaphore = new SemaphoreSlim(1);
                Semaphore.Wait(); // Closing the semaphore immediatelly
                Source = new CancellationTokenSource();
                Count = 1;
                
                this.Limit = Limit;
                limitAquired = false;
                Lock = new SemaphoreSlim(1);
            }

            public async Task WaitTurnAsync()
            {
                if (!limitAquired)
                {
                    try
                    {
                        Lock.Wait();
                        await Limit.WaitAsync(Source.Token); // If cancelled, limitAquired remains false
                        limitAquired = true;
                        Semaphore.Release();
                    }
                    catch
                    {

                    }
                    finally
                    {
                        Lock.Release();
                    }
                }
            }

            public void Dispose()
            {
                Source.Cancel();

                Lock.Wait();
                Semaphore.Dispose();
                Source.Dispose();
                if (limitAquired)
                {
                    Limit.Release();
                }
                Lock.Release();
            }
        }

    }
}
