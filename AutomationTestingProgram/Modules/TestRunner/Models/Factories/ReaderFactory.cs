using AutomationTestingProgram.Core;
using Microsoft.Extensions.Options;

namespace AutomationTestingProgram.Modules.TestRunnerModule
{
    public interface IReaderFactory
    {
        IReader CreateReader(string filePath);
    }
    
    public class ReaderFactory : IReaderFactory
    {

        private readonly CSVEnvironmentGetter _environmentGetter;

        public ReaderFactory(CSVEnvironmentGetter csvGetter)
        {
            _environmentGetter = csvGetter;
        }

        /// <summary>
        /// Creates a new File Reader to read chuncks from a file.
        /// </summary>
        /// <param name="filePath">The filepath of the file to read</param>
        /// <returns></returns>
        public IReader CreateReader(string filePath)
        {
            return new ExcelReader(_environmentGetter, filePath);
        }
    }
}
