
namespace AutomationTestingProgram.Core
{
    /// <summary>
    /// The IO Manager class manages IO Operations.
    /// </summary>
    public static class IOManager
    {
        /* INFO:
         * 
         * As this is a high concurrency environment, to limit concurrent IO operations, this class
         * is used. 
         * 
         * All major IO operations, from reading/writing to files, and DB queries, must aquire a slot
         * before processing. 
         * 
         */

        /// <summary>
        /// To limit high concurrency of IO operations, a limit is used.
        /// </summary>
        private static readonly SemaphoreSlim _maxOperations;

        private static readonly IOSettings _settings;

        static IOManager()
        {
            _settings = AppConfiguration.GetSection<IOSettings>("IO");
            _maxOperations = new SemaphoreSlim(_settings.Limit);
        }

        /// <summary>
        /// Waits until a slot is aquired to perform an IO operation on a file.
        /// </summary>
        /// <returns></returns>
        public static async Task TryAquireSlotAsync()
        {
            await _maxOperations.WaitAsync();
        }

        /// <summary>
        /// Releases a slot for requests to perform IO operations on files.
        /// </summary>
        public static void ReleaseSlot()
        {
            _maxOperations.Release();
        }
    }
}
