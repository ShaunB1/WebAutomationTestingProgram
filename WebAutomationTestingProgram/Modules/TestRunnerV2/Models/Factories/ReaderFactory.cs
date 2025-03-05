using WebAutomationTestingProgram.Modules.TestRunnerV2.Services.File;

namespace WebAutomationTestingProgram.Modules.TestRunnerV2.Models.Factories
{
    public interface IReaderFactory
    {
        IReader CreateReader(string filePath);
    }
    
    public class ReaderFactory : IReaderFactory
    {
        /// <summary>
        /// Creates a new File Reader to read chuncks from a file.
        /// </summary>
        /// <param name="filePath">The filepath of the file to read</param>
        /// <returns></returns>
        public IReader CreateReader(string filePath)
        {
            // return new ExcelReader(filePath);
            return null;
        }
    }
}
