using AutomationTestingProgram.Core;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    /// <summary>
    /// Custom semaphore used for Pause Control
    /// </summary>
    public class AsyncPauseControl
    {
        private SemaphoreSlim _semaphore;
        private bool _paused;
        private readonly object _lock = new object();

        private ICustomLogger RequestLogger;
        private CancellationToken CancellationToken;

        public AsyncPauseControl(ICustomLogger logger, CancellationToken token)
        {
            _semaphore = new SemaphoreSlim(0, 1);
            _paused = false;

            RequestLogger = logger;
            CancellationToken = token;
        }

        public async Task WaitAsync(Func<string, Task> Log)
        {
            if (_paused)
            {
                RequestLogger.LogInformation("Request paused");
                await Log("Request paused");
                bool aquired = await _semaphore.WaitAsync(TimeSpan.FromMinutes(10), CancellationToken);

                if (!aquired)
                {
                    throw new OperationCanceledException("Pause exceeded 10 minutes");
                }
                RequestLogger.LogInformation($"Request unpaused");
                await Log("Request unpaused");
            }
        }

        public void Pause()
        {
            lock (_lock)
            {
                _paused = true;
            }
        }

        public void UnPause()
        {
            lock (_lock)
            {
                if (_paused)
                {
                    _paused = false;
                    try
                    {
                        _semaphore.Release();
                    }
                    catch
                    {

                    }
                }
            }            
        }
    }
}
