namespace AutomationTestingProgram.Backend.Helpers
{   
    /// <summary>
    /// The IO Manager class handles all IO operation needs.
    /// </summary>
    public class FileManager
    {   
        /// <summary>
        /// To limit high concurrency of opened files, a max limit is used
        /// </summary>
        private static readonly SemaphoreSlim _maxRequests;

        static FileManager()
        {
            _maxRequests = new SemaphoreSlim(20);
        }

        /// <summary>
        /// Waits until a slot is aquired to perform an IO operation on a file.
        /// </summary>
        /// <returns></returns>
        public static async Task TryAquireIOSlotAsync()
        {
            await _maxRequests.WaitAsync();
        }

        /// <summary>
        /// Releases a slot for requests to perform IO operations on files.
        /// </summary>
        public static void ReleaseIOSlot()
        {
            _maxRequests.Release();
        }

        public static async Task RetrieveTestSteps(IFormFile file, int[] breakpoints)
        {

        }

        /// <summary>
        /// Validates a given test file.
        /// </summary>
        /// <param name="file">The file to validate</param>
        /// <param name="location">The file location</param>
        /// <returns>Validation result</returns>
        public static async Task ValidateFile(IFormFile file, string location)
        {
            
        }
    }
}
